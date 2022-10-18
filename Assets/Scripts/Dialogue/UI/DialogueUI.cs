using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Dialogue.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Dialogue.UI
{
    public class DialogueUI : MonoBehaviour
    {
        public GameObject dialogueBox;
        public TextMeshProUGUI dialogueText;
        public Image faceLeft, faceRight;
        public TextMeshProUGUI nameLeft, nameRight;
        public GameObject continueBox;

        private void Awake()
        {
            continueBox.SetActive(false);
        }

        private void OnEnable()
        {
            MyEventHandler.ShowDialogue += OnShowDialogueEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.ShowDialogue -= OnShowDialogueEvent;
        }

        private async void OnShowDialogueEvent(DialoguePiece piece)
        {
            if (piece == null)
            {
                dialogueBox.SetActive(false);
                await UniTask.Yield();
                return;
            }

            piece.done = false;

            dialogueBox.SetActive(true);
            continueBox.SetActive(false);

            dialogueText.text = string.Empty;

            if (piece.name.Equals(string.Empty))
            {
                faceLeft.gameObject.SetActive(piece.onLeft);
                faceRight.gameObject.SetActive(!piece.onLeft);
                faceLeft.sprite = piece.faceImage;
                nameLeft.text = piece.onLeft ? piece.name : string.Empty;
                nameRight.text = piece.onLeft ? string.Empty : piece.name;
            }

            await dialogueText.DOText(piece.dialogueText, 1f).WaitForCompletion(true);

            piece.done = true;

            if (piece.needToPause && piece.done)
                continueBox.SetActive(true);
        }
    }
}