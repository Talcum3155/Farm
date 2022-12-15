using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TimeLine
{
    public class DialogueClip : PlayableAsset,ITimelineClipAsset
    {
        public DialogueBehavior dialogueBehavior = new DialogueBehavior();
        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<DialogueBehavior>.Create(graph, dialogueBehavior) ;
        }
    }
}
