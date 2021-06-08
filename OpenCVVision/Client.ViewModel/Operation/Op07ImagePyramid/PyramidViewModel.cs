﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel.Operation
{
    [OperationInfo(7, "图像金字塔", MaterialDesignThemes.Wpf.PackIconKind.LanguagePython)]
    public class PyramidViewModel : OperaViewModelBase
    {
        public ReactiveCommand<Unit, Unit> LaplaceCommand;
        [Reactive] public int DownNum { get; set; }
        [Reactive] public int LaplaceNum { get; set; }
        [Reactive] public int UpNum { get; set; }

        protected override void SetupCommands(CompositeDisposable d)
        {
            base.SetupCommands(d);
            LaplaceCommand = ReactiveCommand.Create(DoLapace);
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);
            this.WhenAnyValue(x => x.DownNum)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Where(i => CanOperat)
                .Do(i => UpdateOutput(true, i))
                .Subscribe();
            this.WhenAnyValue(x => x.UpNum)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Where(i => CanOperat)
                .Do(i => UpdateOutput(false, i))
                .Subscribe();
            this.WhenAnyValue(x => x.LaplaceNum)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Where(i => CanOperat)
                .Do(i => DoLapaceNum(i))
                .Subscribe();
            _imageDataManager.RaiseCurrent();
        }

        #region PrivateFunction

        private void DoLapace()
        {
            SendTime(() =>
            {
                var dstDown = _rt.NewMat();

                dstDown = DownMat(_src.Clone(), DownNum);

                var dstUp = _rt.NewMat();
                dstUp = UpMat(dstDown.Clone(), DownNum);
                var dst = _rt.NewMat();
                var srcNew = dstDown.Resize(dstUp.Size());
                dst = srcNew - dstUp;
                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            });
        }

        private void DoLapaceNum(int num)
        {
            SendTime(() =>
            {
                var dstDown = _rt.NewMat();
                var dstDown1 = _rt.NewMat();
                dstDown = DownMat(_src.Clone(), num + 1);
                dstDown1 = DownMat(_src.Clone(), num);
                var dstUp = _rt.NewMat();
                dstUp = UpMat(dstDown.Clone(), num);
                var dst = _rt.NewMat();
                var srcNew = dstDown1.Resize(dstUp.Size());
                dst = srcNew - dstUp;
                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            });
        }

        private Mat DownMat(Mat src, int num)
        {
            var dst = _rt.T(src.Clone());
            for (int i = 0; i < num; i++)
            {
                dst = dst.PyrDown().Clone();
            }
            return dst;
        }

        private void UpdateOutput(bool isDown, int num)
        {
            SendTime(() =>
            {
                var dst = _rt.NewMat();
                if (isDown)
                {
                    dst = DownMat(_src, num);
                }
                else
                {
                    dst = UpMat(_src, num);
                }
                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            });
        }

        private Mat UpMat(Mat src, int num)
        {
            var dst = _rt.T(src.Clone());
            for (int i = 0; i < num; i++)
            {
                dst = dst.PyrUp().Clone();
            }
            return dst;
        }

        #endregion PrivateFunction
    }
}