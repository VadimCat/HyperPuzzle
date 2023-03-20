using Client.Views.Level;
using UnityEngine;

namespace Views.Cell
{
    [CreateAssetMenu(menuName = "CellViewConfig", fileName = "CellViewConfig")]
    public class CellViewConfig : ScriptableObject
    {
        [SerializeField] private CellView cellView;

        public CellView CellView => cellView;
    }
}