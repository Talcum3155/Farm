using System.Collections.Generic;
using UnityEngine;

namespace Crop.Data
{
    [CreateAssetMenu(fileName = "New CropsSo",menuName = "Crop/CropDetailsSo")]
    public class CropDetailsSo : ScriptableObject
    {
        public List<CropDetails> cropDetailsList;
    }
}
