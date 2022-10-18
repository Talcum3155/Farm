using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dialogue.Data;
using NPC.Logic;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace Dialogue.Logic
{
    [RequireComponent(typeof(NpcMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController: MonoBehaviour
    {
        [SerializeField]private NpcMovement npc;
        public UnityEvent afterTalkEvent;
        public List<DialoguePiece> dialogues = new();
        public bool _canTalk;
        private bool _talking;
        [SerializeField]private GameObject uiSign;

        private Stack<DialoguePiece> _dialogueStack;
        
        private void Start()
        {
            FillDialogueStack();
        }

        private void Update()
        {
            uiSign.SetActive(_canTalk);

            if (_canTalk && Input.GetKeyDown(KeyCode.Space) && !_talking)
            {
                DialogueAsync().Forget();
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player") )
            {
                _canTalk = !npc.moving && npc.interactable;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") )
            {
                _canTalk = false;
            }
        }

        private void FillDialogueStack()
        {
            _dialogueStack = new Stack<DialoguePiece>();
            for (var i = dialogues.Count - 1; i >= 0; i--)
            {
                dialogues[i].done = false;
                _dialogueStack.Push(dialogues[i]);
            }
        }

        private async UniTaskVoid DialogueAsync()
        {
            _talking = true;

            if (_dialogueStack.TryPop(out var result))
            {
                //Send dialogue piece
                MyEventHandler.CallShowDialogue(result);
                //Waiting for the dialogue are showed completely
                await UniTask.WaitUntil(() => result.done);
                _talking = false;
                return;
            }

            MyEventHandler.CallShowDialogue(null);
            FillDialogueStack();
            _talking = false;
        }
    }
}