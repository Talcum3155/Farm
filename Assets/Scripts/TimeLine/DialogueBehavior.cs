using System;
using Dialogue.Data;
using UnityEngine;
using UnityEngine.Playables;
using Utilities;

namespace TimeLine
{
    [Serializable]
    public class DialogueBehavior : PlayableBehaviour
    {
        private PlayableDirector _director;
        public DialoguePiece dialoguePiece;

        public override void OnPlayableCreate(Playable playable)
        {
            _director = playable.GetGraph().GetResolver() as PlayableDirector;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            MyEventHandler.CallShowDialogue(dialoguePiece);

            if (Application.isPlaying)
            {
                if (dialoguePiece.needToPause)
                {
                    //Stop Timeline
                    TimelineManager.Instance.PauseTimeline(_director);
                }
                else
                {
                    MyEventHandler.CallShowDialogue(null);
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (Application.isPlaying)
            {
                TimelineManager.Instance.Done = dialoguePiece.done;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            MyEventHandler.CallShowDialogue(null);
        }

        public override void OnGraphStart(Playable playable)
        {
           MyEventHandler.CallUpdateGameState(GameState.Pause);
        }

        public override void OnGraphStop(Playable playable)
        {
            MyEventHandler.CallUpdateGameState(GameState.GamePlay);
        }
    }
}