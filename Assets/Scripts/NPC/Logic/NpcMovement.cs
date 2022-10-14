using System;
using System.Collections.Generic;
using AStar;
using Cysharp.Threading.Tasks;
using NPC.Data;
using TimeSystem.Logic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace NPC.Logic
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class NpcMovement : MonoBehaviour
    {
        public ScheduleDetailsSo scheduleDetailsSo;
        private SortedSet<ScheduleDetails> _scheduleSet;
        private ScheduleDetails _currentSchedule;

        [SerializeField] private string currentScene;
        private string _targetScene;
        private Vector3Int _currentGridPosition;

        private Vector3Int _targetGridPosition;

        private Vector3Int TargetGridPosition
        {
            set
            {
                _targetGridPosition = value;
                _targetWorldPosition = GetWorldPosition(value);
            }
            get => _targetGridPosition;
        }

        private Vector3 _targetWorldPosition;

        private Vector3Int _nextGridPosition;
        private Vector3 _nextWorldPosition;

        private Grid _grid;
        private bool _initialized;
        private bool _npcMove;

        public string StartScene
        {
            set => currentScene = value;
        }

        public TimeSpan GameTime => TimeManager.Instance.GameTime;

        [Header("移动属性")] public float normalSpeed = 2f;
        public float minSpeed = 1f;
        public float maxSpeed = 3f;
        private Vector3 _dir;
        public bool moving;
        private bool _sceneLoaded;

        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider2D;
        private Animator _animator;
        private AnimationClip _stopAnimationClip;
        public AnimationClip blankAnimation;
        private AnimatorOverrideController _overrideController;

        private bool _eventAnimationPlayable = true;

        private static readonly int DirXHash = Animator.StringToHash("DirX");
        private static readonly int DirYHash = Animator.StringToHash("DirY");
        private static readonly int ExitHash = Animator.StringToHash("Exit");
        private static readonly int EventAnimationHash = Animator.StringToHash("EventAnimation");
        private static readonly int MovingHash = Animator.StringToHash("Moving");

        private Stack<MovementStep> _movementSteps;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _animator = GetComponent<Animator>();
            _movementSteps = new Stack<MovementStep>();

            _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            _animator.runtimeAnimatorController = _overrideController;

            _scheduleSet = new SortedSet<ScheduleDetails>();
            foreach (var schedule in scheduleDetailsSo.scheduleList)
                _scheduleSet.Add(schedule);
        }

        private void OnEnable()
        {
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoadedEvent;
            MyEventHandler.BeforeSceneUnLoad += OnBeforeSceneUnLoadEvent;
            MyEventHandler.GameMinuteEnd += OnGameMinuteEndEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadedEvent;
            MyEventHandler.BeforeSceneUnLoad -= OnBeforeSceneUnLoadEvent;
            MyEventHandler.GameMinuteEnd -= OnGameMinuteEndEvent;
        }

        private void Update()
        {
            if (_sceneLoaded)
                SwitchAnimation();
        }

        private void FixedUpdate()
        {
            if (_sceneLoaded)
                Movement();
        }

        private void CheckVisible()
        {
            if (currentScene.Equals(SceneManager.GetActiveScene().name))
            {
                SetActiveInScene();
                return;
            }

            SetInactiveInScene();
        }

        private void InitNpc()
        {
            _targetScene = currentScene;

            //Keep npc is in the center of this grip
            _currentGridPosition = _grid.WorldToCell(transform.position);
            _targetWorldPosition
                = transform.position
                    = new Vector3(_currentGridPosition.x + Settings.GridCellSize / 2f,
                        _currentGridPosition.y + Settings.GridCellSize / 2f, 0);
        }

        private void Movement()
        {
            if (_npcMove)
                return;

            if (_movementSteps.Count > 0)
            {
                var step = _movementSteps.Pop();
                currentScene = step.sceneName;
                CheckVisible();

                _nextGridPosition = (Vector3Int)step.gridCoordinate;
                var stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(_nextGridPosition, stepTime).Forget();
                return;
            }

            if (!moving && _eventAnimationPlayable)
            {
                PlayStopAnimation().Forget();
                _eventAnimationPlayable = false;
            }
        }

        private async UniTaskVoid MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
        {
            _npcMove = true;
            _nextWorldPosition = GetWorldPosition(gridPos);
            //There is still remain time for the next step
            if (stepTime > GameTime)
            {
                //Time required to move to the next step
                var timeForNextStep = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
                //Move distance of this step and next step
                var distance = Vector3.Distance(transform.position, _nextWorldPosition);

                var speed = Math.Max(minSpeed, distance / timeForNextStep / Settings.SecondThreshold);

                if (speed <= maxSpeed)
                {
                    //Judgment distance by pixel-level
                    while (Vector3.Distance(transform.position, _nextWorldPosition) > Settings.PixelSize)
                    {
                        Debug.Log("移动中...");
                        _dir = (_nextWorldPosition - transform.position).normalized;

                        var posOffset = new Vector2(
                            _dir.x * speed * Time.fixedDeltaTime,
                            _dir.y * speed * Time.fixedDeltaTime
                        );

                        _rigidbody2D.MovePosition(_rigidbody2D.position + posOffset);
                        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
                    }
                }
            }

            //There is no time for next step so teleport npc to another pos
            _rigidbody2D.position = _nextWorldPosition;
            _currentGridPosition = gridPos;
            _nextGridPosition = _currentGridPosition;

            _npcMove = false;
        }

        public void BuildPath(ScheduleDetails schedule)
        {
            _movementSteps.Clear();
            _currentSchedule = schedule;
            TargetGridPosition = (Vector3Int)schedule.targetGridPosition;
            _stopAnimationClip = schedule.animationClip;

            if (schedule.targetScene.Equals(currentScene))
            {
                AStarCore.Instance.BuildPath(schedule.targetScene, (Vector2Int)_currentGridPosition,
                    schedule.targetGridPosition,
                    _movementSteps);
            }

            if (_movementSteps.Count > 1)
                UpdateTimeOnPtah();
        }

        /// <summary>
        /// Fill data for each steps
        /// </summary>
        private void UpdateTimeOnPtah()
        {
            MovementStep previousStep = null;
            var currentTime = GameTime;

            foreach (var step in _movementSteps)
            {
                Debug.Log("构建每一步的行动");
                previousStep ??= step;

                step.hour = currentTime.Hours;
                step.minute = currentTime.Minutes;
                step.second = currentTime.Seconds;

                //Time for one step that walk rightly or diagonally
                TimeSpan gridMovementStepTime;
                if (MoveInDiagonal(step, previousStep))
                {
                    gridMovementStepTime = new TimeSpan(0, 0,
                        (int)(Settings.GridCellDiagonalSize / normalSpeed / Settings.SecondThreshold));
                }
                else
                {
                    gridMovementStepTime = new TimeSpan(0, 0,
                        (int)(Settings.GridCellSize / normalSpeed / Settings.SecondThreshold));
                }

                //Time info of next step
                currentTime = currentTime.Add(gridMovementStepTime);
            }
        }

        /// <summary>
        /// Determine if the npc need to walk diagonally
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="previousStep"></param>
        /// <returns></returns>
        private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
        {
            return currentStep.gridCoordinate.x != previousStep.gridCoordinate.x &&
                   currentStep.gridCoordinate.y != previousStep.gridCoordinate.y;
        }

        /// <summary>
        /// Get world position in grid cell and turn it to the center of world pos
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        private Vector3 GetWorldPosition(Vector3Int gridPos)
        {
            var worldPos = _grid.CellToWorld(gridPos);
            return new Vector3(worldPos.x + Settings.GridCellSize / 2f, worldPos.y + Settings.GridCellSize / 2f);
        }

        private void SwitchAnimation()
        {
            moving = transform.position != _targetWorldPosition;

            _animator.SetBool(MovingHash, moving);
            if (moving)
            {
                _animator.SetBool(ExitHash, true);
                _animator.SetFloat(DirXHash, _dir.x);
                _animator.SetFloat(DirYHash, _dir.y);
                return;
            }

            _animator.SetBool(ExitHash, false);
        }

        private async UniTask PlayStopAnimation()
        {
            _animator.SetFloat(DirXHash, 0);
            _animator.SetFloat(DirYHash, -1);

            await UniTask.Delay(TimeSpan.FromSeconds(Settings.AnimationBreakTime));
            if (_stopAnimationClip != null)
            {
                //Find blankAnimationClip in AnimationOverrideController and change it to stopAnimationClip
                _overrideController[blankAnimation] = _stopAnimationClip;
                _animator.SetBool(EventAnimationHash, true);
                await UniTask.Yield();
                _animator.SetBool(EventAnimationHash, false);
                _eventAnimationPlayable = true;
                return;
            }

            //Find stopAnimationClip in AnimationOverrideController and change it to blankAnimationClip
            _overrideController[_stopAnimationClip] = blankAnimation;
            _animator.SetBool(EventAnimationHash, false);
            _eventAnimationPlayable = true;
        }

        #region 设置NPC的显示情况

        /// <summary>
        /// Npc is not in this scene, hide it
        /// </summary>
        private void SetActiveInScene()
        {
            _spriteRenderer.enabled = true;
            _boxCollider2D.enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
        }

        /// <summary>
        /// Npc is in this scene, display it
        /// </summary>
        private void SetInactiveInScene()
        {
            _spriteRenderer.enabled = false;
            _boxCollider2D.enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
        }

        #endregion

        #region 事件绑定

        private void OnAfterSceneLoadedEvent()
        {
            _grid = FindObjectOfType<Grid>();
            CheckVisible();
            _sceneLoaded = true;

            //Initialization npc only once time
            if (_initialized)
                return;
            InitNpc();
            _initialized = true;
        }

        private void OnBeforeSceneUnLoadEvent()
        {
            _sceneLoaded = false;
        }

        private void OnGameMinuteEndEvent(int minute, int hour, int day, Season season)
        {
            //Match schedule by this formula
            var time = hour * 100 + minute;

            ScheduleDetails matchSchedule = null;
            foreach (var details in _scheduleSet)
            {
                if (details.Time == time)
                {
                    if (details.season != season)
                        continue;
                    if (details.day != 0 && details.day != day)
                        continue;

                    matchSchedule = details;
                    break;
                }

                //ScheduleSet sorted from smallest to largest, if (details.Time > time) means there is no suitable schedule after this
                if (details.Time > time)
                    break;
            }

            if (matchSchedule != null)
                BuildPath(matchSchedule);
        }

        #endregion
    }
}