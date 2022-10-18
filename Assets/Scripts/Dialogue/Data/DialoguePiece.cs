using System;
using UnityEngine;

namespace Dialogue.Data
{
    [Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")] public Sprite faceImage;
        public bool onLeft;
        public string name;
        [TextArea] public string dialogueText;
        public bool needToPause;
        public bool done;
    }
}
