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

#nullable enable
using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace DeusaldUnityTools
{
    [PublicAPI]
    public abstract class Tween
    {
        /// <summary>The is paused property will return whether the Tween is paused while also allowing you to pause the Tween.</summary>
        public bool IsPaused { get; set; }

        /// <summary>The duration of the Tween in seconds defines how long the Tween will take to complete. When the duration is not set, the Tween will complete instantly.</summary>
        public float Duration { get; set; }

        /// <summary>The delay of the Tween in seconds defines how long the Tween will wait before starting. To change the behavior of how the delay will affect the Tween before it starts, you can change the Fill Mode. When the delay is not set, the Tween will start instantly.</summary>
        public float? Delay { get; set; }

        /// <summary>The ping pong interval defines how long the Tween will wait before playing the animation backwards after the animation has finished. When the ping pong interval is not set, the Tween will play the animation backwards instantly after the animation has finished.</summary>
        public float? PingPongInterval { get; set; }

        /// <summary>The repeat interval defines how long the Tween will wait before repeating itself. When the repeat interval is not set, the Tween will repeat itself instantly after the animation has finished.</summary>
        public float? RepeatInterval { get; set; }

        /// <summary>The use unscaled time option defines whether the Tween will use the unscaled time. When the use-unscaled time option is not set, the Tween will use the scaled time.</summary>
        public bool UseUnscaledTime { get; set; }

        /// <summary>The ping pong option defines whether the Tween will play the animation backwards after the animation has finished. When the ping pong option is not set, the Tween will not play the animation backwards after the animation has finished.</summary>
        public bool UsePingPong { get; set; }

        /// <summary>The infinite option defines whether the Tween will loop forever. When the Tween is set to loop forever, the Loops option will be ignored. When the infinite option is not set, the Tween will not loop forever.</summary>
        public bool IsInfinite { get; set; }

        /// <summary>The number of times the Tween will loop defines how many times the Tween will repeat itself. When the Tween is using a Ping-Pong loop type, the Tween has to play both the forward and backward animation to count as one loop. When Infinite is set, the Tween will loop forever and the loop count will be ignored. When the number of loops is not set, the Tween will not loop.</summary>
        public int? Loops { get; set; }

        /// <summary>The offset defines on which time the Tween will start. When the offset is not set, the Tween will start at the beginning.</summary>
        public float? Offset { get; set; }

        /// <summary>The ease type defines how the Tween will animate. If an Animation Curve is set, the Ease Type won't be used. When the ease type is not set, the Tween will animate linearly.</summary>
        public EaseType EaseType { get; set; }

        /// <summary>The fill mode defines how the Tween will behave before the Tween has started and after the Tween has ended. When the fill mode is not set, the fill mode will be set to Backward.</summary>
        public FillMode FillMode { get; set; } = FillMode.Backwards;

        /// <summary>The animation curve defines how the Tween will animate. The animation curve can be used to create custom ease types. When the animation curve is not set, the Tween will animate, according to the Ease Type.</summary>
        public AnimationCurve? AnimationCurve { get; set; }

        /// <summary>The "don't invoke when destroyed option" defines whether the Tween should invoke the delegates when the component is no longer present in the scene. When not set, all delegates will be invoked even when the component is destroyed.</summary>
        public bool DontInvokeWhenDestroyed { get; set; }
        
        internal bool IsDecommissioned { get; set; }

        /// <summary>The cancel method will cancel the Tween. When the Tween is canceled, the OnCancel and OnFinally delegates will be invoked.</summary>
        public abstract void Cancel();

        /// <summary>The await decommission method will return an enumerator that will await the decommission of the Tween. This can be used in coroutines to wait for the Tween to finish or be canceled.</summary>
        public abstract IEnumerator AwaitDecommission();

        /// <summary>The await decommission async method will return an awaitable that will await the decommission of the Tween. This can be used in async methods to wait for the Tween to finish or be canceled.</summary>
        public abstract Awaitable AwaitDecommissionAsync();

        internal abstract void Start();
        internal abstract void Update();
    }

    [PublicAPI]
    public abstract class Tween<TComponent, TData> : Tween where TComponent : Component
    {
        /// <summary>The "From" value defines the starting value of the Tween. When the "From" value is not set, the Tween will use the current value of the property.</summary>
        public TData? From { get; set; }

        /// <summary>The "To" value defines the end value of the Tween. When the to value is not set, the Tween will use the current value of the property.</summary>
        public TData? To { get; set; }

        /// <summary>The "On Add" delegate will be invoked when the Tween has been added to a GameObject.</summary>
        public Action<Tween>? OnAdd { get; set; }

        /// <summary>The on start delegate will be invoked when the Tween has started.</summary>
        public Action<Tween>? OnStart { get; set; }

        /// <summary>The on update delegate will be invoked when the Tween has updated.</summary>
        public Action<Tween, TData>? OnUpdate { get; set; }

        /// <summary>The on end delegate will be invoked when the Tween has ended.</summary>
        public Action<Tween>? OnEnd { get; set; }

        /// <summary>The on cancel delegate will be invoked when the Tween has been canceled.</summary>
        public Action<Tween>? OnCancel { get; set; }

        /// <summary>The on finally delegate will be invoked when the Tween has ended or has been canceled.</summary>
        public Action<Tween>? OnFinally { get; set; }

        /// <summary>Component that will be evaluated under Tween. If null, then all Updates should be performed using the "On Update" callback.</summary>
        public TComponent? Component { get; set; }

        private bool                  _WithComponent;
        private bool                  _OnStartTriggered;
        private float?                _HaltTime;
        private int?                  _Loops;
        private float                 _Time;
        private bool                  _IsForwards;
        private bool                  _DidReachEnd;
        private EaseFunctionDelegate? _EaseFunction;
        private TData?                _InitialValue;
        private TData?                _From;
        private TData?                _To;

        protected abstract TData CurrentFromComponent();
        protected abstract TData Lerp(TData from, TData to, float time);
        protected abstract void  ApplyToComponent(TData value);

        internal override void Start()
        {
            Duration         = Duration > 0 ? Duration : 0.00001f;
            IsDecommissioned = false;

            _OnStartTriggered = false;
            _HaltTime         = Delay;
            _Loops            = IsInfinite ? -1 : Loops;
            _Time             = Offset ?? 0f;
            _IsForwards       = true;
            _DidReachEnd      = false;
            _EaseFunction     = AnimationCurve != null ? new AnimationCurve(AnimationCurve.keys).Evaluate : EaseFunctions.GetFunction(EaseType);
            _InitialValue     = CurrentFromComponent();
            _From             = From ?? _InitialValue;
            _To               = To ?? _InitialValue;

            OnAdd?.Invoke(this);

            if (FillMode is not (FillMode.Both or FillMode.Forwards)) return;
            ApplyToComponent(_From);
            OnUpdate?.Invoke(this, _From);
        }

        internal override void Update()
        {
            if (_WithComponent && Component == null)
            {
                Cancel();
                return;
            }

            if (IsPaused) return;

            float deltaTime = UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (_HaltTime.HasValue)
            {
                _HaltTime -= deltaTime;
                if (_HaltTime <= 0) _HaltTime = null;
                else return;
            }

            if (!_OnStartTriggered)
            {
                OnStart?.Invoke(this);
                _OnStartTriggered = true;
            }

            _Time += _IsForwards ? deltaTime : -deltaTime;

            if (_Time >= Duration)
            {
                _Time = Duration;

                if (UsePingPong)
                {
                    _IsForwards = false;
                    _HaltTime   = PingPongInterval ?? _HaltTime;
                }
                else
                {
                    _DidReachEnd = true;
                    _HaltTime    = RepeatInterval ?? _HaltTime;
                }
            }
            else if (UsePingPong && _Time < 0)
            {
                _Time        = 0;
                _IsForwards  = true;
                _DidReachEnd = true;
                _HaltTime    = RepeatInterval ?? _HaltTime;
            }

            float easedTime = _EaseFunction!(_Time / Duration);
            TData value     = Lerp(_From!, _To!, easedTime);
            ApplyToComponent(value);
            OnUpdate?.Invoke(this, value);

            if (!_DidReachEnd) return;

            if (_Loops is > 1 or -1)
            {
                _DidReachEnd = false;
                _Time        = 0;
                if (_Loops > 1) _Loops -= 1;
            }
            else
            {
                if (FillMode is FillMode.Forwards or FillMode.None)
                {
                    ApplyToComponent(_InitialValue!);
                    OnUpdate?.Invoke(this, _InitialValue!);
                }

                OnEnd?.Invoke(this);
                OnFinally?.Invoke(this);
                IsDecommissioned = true;
            }
        }

        /// <summary>The cancel method will cancel the Tween. When the Tween is cancelled, the OnCancel and OnFinally delegates will be invoked.</summary>
        public override void Cancel()
        {
            if (!_WithComponent || !DontInvokeWhenDestroyed || Component != null)
            {
                OnCancel?.Invoke(this);
                OnFinally?.Invoke(this);
            }

            IsDecommissioned = true;
        }

        /// <summary>The cancel method will cancel the Tween. When the Tween is canceled, the OnCancel and OnFinally delegates will be invoked.</summary>
        public override IEnumerator AwaitDecommission()
        {
            while (!IsDecommissioned) yield return null;
        }

        /// <summary>The await decommission async method will return an awaitable that will await the decommission of the Tween. This can be used in async methods to wait for the Tween to finish or be canceled.</summary>
        public override async Awaitable AwaitDecommissionAsync()
        {
            while (!IsDecommissioned) await Awaitable.EndOfFrameAsync();
        }

        // <summary> Cast special generic tween type onto base one. </summary>
        public Tween AsTween()
        {
            return this;
        }

        /// <summary>The "From" value defines the starting value of the Tween. When the "From" value is not set, the Tween will use the current value of the property.
        /// The "To" value defines the end value of the Tween. When the to value is not set, the Tween will use the current value of the property.</summary>
        public Tween<TComponent, TData> SetTarget(TData from, TData to)
        {
            From = from;
            To   = to;
            return this;
        }

        /// <summary>The "To" value defines the end value of the Tween. When the to value is not set, the Tween will use the current value of the property.</summary>
        public Tween<TComponent, TData> SetTarget(TData to)
        {
            To = to;
            return this;
        }

        /// <summary>The "From" value defines the starting value of the Tween. When the "From" value is not set, the Tween will use the current value of the property.
        /// The "To" value defines the end value of the Tween. When the to value is not set, the Tween will use the current value of the property.</summary>
        public Tween<TComponent, TData> SetTarget(TComponent component, TData from, TData to)
        {
            _WithComponent = true;
            Component      = component;
            From           = from;
            To             = to;
            return this;
        }

        /// <summary>The "To" value defines the end value of the Tween. When the to value is not set, the Tween will use the current value of the property.</summary>
        public Tween<TComponent, TData> SetTarget(TComponent component, TData to)
        {
            _WithComponent = true;
            Component      = component;
            To             = to;
            return this;
        }

        /// <summary>The duration of the Tween in seconds defines how long the Tween will take to complete. When the duration is not set, the Tween will complete instantly.</summary>
        public Tween<TComponent, TData> SetDuration(float duration)
        {
            Duration = duration;
            return this;
        }

        /// <summary>The delay of the Tween in seconds defines how long the Tween will wait before starting. To change the behavior of how the delay will affect the Tween before it starts, you can change the Fill Mode. When the delay is not set, the Tween will start instantly.</summary>
        public Tween<TComponent, TData> AddDelay(float delay)
        {
            Delay = delay;
            return this;
        }

        /// <summary>The ping pong interval defines how long the Tween will wait before playing the animation backwards after the animation has finished. When the ping pong interval is not set, the Tween will play the animation backwards instantly after the animation has finished.</summary>
        public Tween<TComponent, TData> SetPingPongInterval(float interval)
        {
            PingPongInterval = interval;
            return this;
        }

        /// <summary>The repeat interval defines how long the Tween will wait before repeating itself. When the repeat interval is not set, the Tween will repeat itself instantly after the animation has finished.</summary>
        public Tween<TComponent, TData> SetRepeatInterval(float interval)
        {
            RepeatInterval = interval;
            return this;
        }

        /// <summary>The use unscaled time option defines whether the Tween will use the unscaled time. When the use-unscaled time option is not set, the Tween will use the scaled time.</summary>
        public Tween<TComponent, TData> UnscaledTime(bool useUnscaledTime = true)
        {
            UseUnscaledTime = useUnscaledTime;
            return this;
        }

        /// <summary>The ping pong option defines whether the Tween will play the animation backwards after the animation has finished. When the ping pong option is not set, the Tween will not play the animation backwards after the animation has finished.</summary>
        public Tween<TComponent, TData> PingPong(bool usePingPong = true)
        {
            UsePingPong = usePingPong;
            return this;
        }

        /// <summary>The infinite option defines whether the Tween will loop forever. When the Tween is set to loop forever, the Loops option will be ignored. When the infinite option is not set, the Tween will not loop forever.</summary>
        public Tween<TComponent, TData> Infinite(bool isInfinite = true)
        {
            IsInfinite = isInfinite;
            return this;
        }

        /// <summary>The number of times the Tween will loop defines how many times the Tween will repeat itself. When the Tween is using a Ping-Pong loop type, the Tween has to play both the forward and backward animation to count as one loop. When Infinite is set, the Tween will loop forever and the loop count will be ignored. When the number of loops is not set, the Tween will not loop.</summary>
        public Tween<TComponent, TData> SetLoops(int loops)
        {
            Loops = loops;
            return this;
        }

        /// <summary>The offset defines on which time the Tween will start. When the offset is not set, the Tween will start at the beginning.</summary>
        public Tween<TComponent, TData> SetOffset(float offset)
        {
            Offset = offset;
            return this;
        }

        /// <summary>The ease type defines how the Tween will animate. If an Animation Curve is set, the Ease Type won't be used. When the ease type is not set, the Tween will animate linearly.</summary>
        public Tween<TComponent, TData> SetEase(EaseType easeType)
        {
            EaseType = easeType;
            return this;
        }

        /// <summary>The fill mode defines how the Tween will behave before the Tween has started and after the Tween has ended. When the fill mode is not set, the fill mode will be set to Backward.</summary>
        public Tween<TComponent, TData> SetFillMode(FillMode fillMode)
        {
            FillMode = fillMode;
            return this;
        }

        /// <summary>The animation curve defines how the Tween will animate. The animation curve can be used to create custom ease types. When the animation curve is not set, the Tween will animate, according to the Ease Type.</summary>
        public Tween<TComponent, TData> SetAnimationCurve(AnimationCurve animationCurve)
        {
            AnimationCurve = animationCurve;
            return this;
        }

        /// <summary>The duration of the Tween in seconds defines how long the Tween will take to complete. When the duration is not set, the Tween will complete instantly.</summary>
        public Tween<TComponent, TData> SetDontInvokeWhenDestroyed(bool dontInvokeWhenDestroyed)
        {
            DontInvokeWhenDestroyed = dontInvokeWhenDestroyed;
            return this;
        }

        /// <summary>The "On Add" delegate will be invoked when the Tween has been added to a GameObject.</summary>
        public Tween<TComponent, TData> AddOnAddCallback(Action<Tween> callback)
        {
            OnAdd = callback;
            return this;
        }

        /// <summary>The on start delegate will be invoked when the Tween has started.</summary>
        public Tween<TComponent, TData> AddOnStartCallback(Action<Tween> callback)
        {
            OnStart = callback;
            return this;
        }

        /// <summary>The on update delegate will be invoked when the Tween has updated.</summary>
        public Tween<TComponent, TData> AddOnUpdateCallback(Action<Tween, TData> callback)
        {
            OnUpdate = callback;
            return this;
        }

        /// <summary>The on end delegate will be invoked when the Tween has ended.</summary>
        public Tween<TComponent, TData> AddOnEndCallback(Action<Tween> callback)
        {
            OnEnd = callback;
            return this;
        }

        /// <summary>The on cancel delegate will be invoked when the Tween has been canceled.</summary>
        public Tween<TComponent, TData> AddOnCancelCallback(Action<Tween> callback)
        {
            OnCancel = callback;
            return this;
        }

        /// <summary>The on finally delegate will be invoked when the Tween has ended or has been canceled.</summary>
        public Tween<TComponent, TData> AddOnFinallyCallback(Action<Tween> callback)
        {
            OnFinally = callback;
            return this;
        }
    }
}