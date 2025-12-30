using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace GenBall.UI
{
    public class CellViewSpawner : MonoBehaviour
    {
        private readonly List<object> _args = new();
        private readonly Dictionary<ICellView,GameObject> _cellViewMap = new();
        private readonly List<ICellView> _cachedCellViews = new();
        [SerializeField] private GameObject cellViewPrefab;
        public int CellCount=>_args.Count;

        public void SetDate(IEnumerable<object> args)
        {
            _args.Clear();
            _args.AddRange(args);
            Refresh();
        }
        public void Refresh()
        {
            int addCount=_args.Count-_cellViewMap.Count;
            if(addCount>0) LoadCellViews(addCount);
            _cachedCellViews.Clear();
            _cachedCellViews.AddRange(_cellViewMap.Keys);
            for (int index = 0; index < _cachedCellViews.Count; index++)
            {
                var cellView = _cachedCellViews[index];
                if (index < _args.Count)
                {
                    var args = _args[index];
                    cellView.OnRefresh(index,args);
                }
                _cellViewMap[cellView].SetActive(index<_args.Count);
            }
        }

        private void LoadCellViews(int count)
        {
            for (int i = 0; i < count; i++)
            {
                LoadCellView();
            }
        }
        private void LoadCellView()
        {
            if(cellViewPrefab == null) throw new Exception("gzp CellViewPrefab is null");
            var go= Instantiate(cellViewPrefab, transform);
            if (go.TryGetComponent(out ICellView cellView))
            {
                _cellViewMap.Add(cellView,go);
                go.SetActive(false);
            }
            else
            {
                throw new Exception("gzp CellViewPrefab ±ØÐë¹ÒÓÐICellView");
            }
        }
    }
}