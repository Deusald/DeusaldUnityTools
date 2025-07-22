// Tweens module based on unity-tweens by Jeffrey Lanters - https://github.com/jeffreylanters/unity-tweens
// MIT License
// Copyright (c) 2020 Jeffrey Lanters

// DeusaldUnityTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// MIT License

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace DeusaldUnityTools
{
    [PublicAPI]
    public class TweenEngine : MonoBehaviour, ITweenEngine
    {
        private readonly List<Tween>         _Tweens    = new();
        private readonly List<TweenSequence> _Sequences = new();

        public void RunTween(Tween tween)
        {
            tween.Start();
            _Tweens.Add(tween);
        }

        public void RunSequence(TweenSequence sequence)
        {
            sequence.TweenEngine = this;
            sequence.Start();
            _Sequences.Add(sequence);
        }

        private void LateUpdate()
        {
            for (int x = 0; x < _Tweens.Count; ++x)
            {
                Tween tween = _Tweens[x];

                if (tween.IsDecommissioned)
                {
                    _Tweens.Remove(tween);
                    --x;
                    continue;
                }

                tween.Update();
            }

            for (int x = 0; x < _Sequences.Count; ++x)
            {
                TweenSequence sequence = _Sequences[x];

                if (sequence.IsDecommissioned)
                {
                    _Sequences.Remove(sequence);
                    --x;
                    continue;
                }

                sequence.Update();
            }
        }
    }
}