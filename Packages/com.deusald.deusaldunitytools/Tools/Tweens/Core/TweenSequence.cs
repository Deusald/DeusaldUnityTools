using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeusaldUnityTools
{
    public class TweenSequence
    {
        internal ITweenEngine TweenEngine      { get; set; }
        internal bool         IsDecommissioned { get; set; }

        private int _CurrentTweenLevel = -1;

        private readonly List<List<Tween>>              _Tweens         = new();
        private readonly Dictionary<float, List<Tween>> _InsertedTweens = new();

        private readonly List<Tween> _WaitingForTweens  = new();
        private readonly List<Tween> _AllInsertedTweens = new();

        internal void Start()
        {
            _WaitingForTweens.Clear();
            _AllInsertedTweens.Clear();
            _CurrentTweenLevel = -1;
            
            foreach (KeyValuePair<float, List<Tween>> pairs in _InsertedTweens)
            {
                foreach (Tween targetTween in pairs.Value)
                {
                    _AllInsertedTweens.Add(new FloatTween().SetDuration(pairs.Key).AddOnEndCallback(_ =>
                    {
                        _AllInsertedTweens.Add(targetTween);
                        TweenEngine.RunTween(targetTween);
                    }));
                }
            }

            foreach (Tween tween in _AllInsertedTweens) TweenEngine.RunTween(tween);
        }

        internal void Update()
        {
            if (IsDecommissioned) return;
            if (_WaitingForTweens.Count != 0 && _WaitingForTweens.Any(t => !t.IsDecommissioned)) return;
            ++_CurrentTweenLevel;
            if (_Tweens.Count <= _CurrentTweenLevel)
            {
                IsDecommissioned = true;
                return;
            }

            _WaitingForTweens.Clear();
            
            foreach (Tween tween in _Tweens[_CurrentTweenLevel])
            {
                _WaitingForTweens.Add(tween);
                TweenEngine.RunTween(tween);
            }
        }

        public TweenSequence Append(Tween tween)
        {
            _Tweens.Add(new List<Tween> { tween });
            return this;
        }

        public TweenSequence AppendCallback(Action callback)
        {
            _Tweens.Add(new List<Tween>
            {
                new FloatTween().AddOnEndCallback(_ => callback())
            });
            return this;
        }

        public TweenSequence AppendInterval(float time)
        {
            _Tweens.Add(new List<Tween>
            {
                new FloatTween().SetDuration(time)
            });
            return this;
        }

        public TweenSequence Insert(float time, Tween tween)
        {
            if (_InsertedTweens.TryGetValue(time, out List<Tween> tweens)) tweens.Add(tween);
            else _InsertedTweens.Add(time, new List<Tween> { tween });
            return this;
        }

        public TweenSequence InsertCallback(float time, Action callback)
        {
            if (_InsertedTweens.TryGetValue(time, out List<Tween> tweens)) tweens.Add(new FloatTween().AddOnEndCallback(_ => callback()));
            else _InsertedTweens.Add(time, new List<Tween> { new FloatTween().AddOnEndCallback(_ => callback()) });
            return this;
        }

        public TweenSequence Join(Tween tween)
        {
            if (_Tweens.Count == 0) Append(tween);
            else _Tweens[^1].Add(tween);
            return this;
        }

        public void Cancel()
        {
            IsDecommissioned = true;
            foreach (Tween tween in _WaitingForTweens) tween.Cancel();
            foreach (Tween tween in _AllInsertedTweens) tween.Cancel();
        }
        
        /// <summary>The cancel method will cancel the Tween. When the Tween is canceled, the OnCancel and OnFinally delegates will be invoked.</summary>
        public IEnumerator AwaitDecommission()
        {
            while (!IsDecommissioned) yield return null;
        }

        /// <summary>The await decommission async method will return an awaitable that will await the decommission of the Tween. This can be used in async methods to wait for the Tween to finish or be canceled.</summary>
        public async Awaitable AwaitDecommissionAsync()
        {
            while (!IsDecommissioned) await Awaitable.EndOfFrameAsync();
        }
    }
}