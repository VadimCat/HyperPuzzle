using UnityEngine;

namespace Client.Views.Level
{
    [CreateAssetMenu]
    public class LevelsConfig : LevelsViewDataStorageBase<LevelConfig>
    {
        [SerializeField] private CellViewHolder cellHolder;
        [SerializeField] private CellView cellView;

        public CellViewHolder CellHolder => cellHolder;
        public CellView CellView => cellView;
    }
}