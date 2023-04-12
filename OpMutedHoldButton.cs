using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace RestartButton
{
    // Copy of OpHoldButton with removed sound loop, sound loop requires full Menu, which PauseMenu isn't
    public class OpMutedHoldButton : UIfocusable
    {
        protected readonly FLabel _label;

        protected float _filled;

        protected float _pulse;

        protected readonly float _fillTime;

        protected bool _hasSignalled;

        protected int _releaseCounter;

        protected readonly FSprite[] _circles;

        protected readonly DyeableRect _rect;

        protected readonly DyeableRect _rectH;

        protected readonly GlowGradient _glow;

        protected readonly FSprite _fillSprite;

        private string _text;

        public Color colorEdge = MenuColorEffect.rgbMediumGrey;

        public Color colorFill = MenuColorEffect.rgbBlack;

        private bool _isProgress;

        public byte progressDeci = 2;

        public event OnSignalHandler OnPressInit;

        public event OnSignalHandler OnPressDone;

        public event OnSignalHandler OnClick;

        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    Change();
                }
            }
        }

        protected override Rect FocusRect
        {
            get
            {
                Rect focusRect = base.FocusRect;
                if (!isRectangular)
                {
                    focusRect.x -= 15f;
                    focusRect.y -= 15f;
                    focusRect.width += 30f;
                    focusRect.height += 30f;
                }
                return focusRect;
            }
        }

        public float progress { get; private set; }

        public OpMutedHoldButton(Vector2 pos, float rad, string displayText, float fillTime = 80f) : base(pos, Mathf.Max(40f, rad))
        {
            OnPressInit += FocusMoveDisallow;
            OnClick += FocusMoveDisallow;
            OnPressDone += FocusMoveDisallow;
            _fillTime = Mathf.Max(0f, fillTime);
            _text = displayText;
            _circles = new FSprite[5];
            _circles[0] = new FSprite("Futile_White", true)
            {
                shader = Custom.rainWorld.Shaders["VectorCircleFadable"]
            };
            _circles[1] = new FSprite("Futile_White", true)
            {
                shader = Custom.rainWorld.Shaders["VectorCircle"]
            };
            _circles[2] = new FSprite("Futile_White", true)
            {
                shader = Custom.rainWorld.Shaders["HoldButtonCircle"]
            };
            _circles[3] = new FSprite("Futile_White", true)
            {
                shader = Custom.rainWorld.Shaders["VectorCircle"]
            };
            _circles[4] = new FSprite("Futile_White", true)
            {
                shader = Custom.rainWorld.Shaders["VectorCircleFadable"]
            };
            for (int i = 0; i < _circles.Length; i++)
            {
                myContainer.AddChild(_circles[i]);
                _circles[i].SetPosition(base.rad, base.rad);
            }
            _label = FLabelCreate(text, false);
            myContainer.AddChild(_label);
            FLabelPlaceAtCenter(_label, Vector2.zero, 2f * base.rad * Vector2.one);
        }

        public OpMutedHoldButton(Vector2 pos, Vector2 size, string displayText, float fillTime = 80f) : base(pos, size)
        {
            _fillTime = Mathf.Max(0f, fillTime);
            _size = new Vector2(Mathf.Max(24f, size.x), Mathf.Max(24f, size.y));
            _text = displayText;
            _rect = new DyeableRect(myContainer, Vector2.zero, base.size, true);
            _rectH = new DyeableRect(myContainer, Vector2.zero, base.size, false);
            _fillSprite = new FSprite("pixel", true)
            {
                anchorX = 0f,
                anchorY = 0f,
                x = _rect.sprites[8].x,
                y = _rect.sprites[8].y,
                scaleX = 9f,
                scaleY = _rect.sprites[8].scaleY,
                color = colorEdge,
                alpha = 1f
            };
            myContainer.AddChild(_fillSprite);
            _glow = new GlowGradient(myContainer, Vector2.zero, Vector2.one, 0.5f);
            _glow.Hide();
            _label = FLabelCreate(text, false);
            FLabelPlaceAtCenter(_label, Vector2.zero, base.size);
            myContainer.AddChild(_label);
        }

        protected override string DisplayDescription()
        {
            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }
            if (_isProgress)
            {
                return "";
            }
            return OptionalText.GetText(MenuMouseMode ? OptionalText.ID.OpHoldButton_MouseTuto : OptionalText.ID.OpHoldButton_NonMouseTuto);
        }

        public override void Reset()
        {
            base.Reset();
            _filled = 0f;
            _pulse = 0f;
            _hasSignalled = false;
            held = false;
            SetProgress(-1f);
        }

        protected override void Unload()
        {
            base.Unload();
        }

        protected override void Change()
        {
            base.Change();
            if (!isRectangular)
            {
                FLabelPlaceAtCenter(_label, Vector2.zero, 2f * rad * Vector2.one);
            }
            else
            {
                _size = new Vector2(Mathf.Max(24f, size.x), Mathf.Max(24f, size.y));
                FLabelPlaceAtCenter(_label, Vector2.zero, size);
                _rect.size = size;
                _rectH.size = size;
            }
            if (!string.IsNullOrEmpty(text) || !_isProgress)
            {
                _label.text = text;
                return;
            }
            _label.text = progress.ToString("N" + Custom.IntClamp(progressDeci, 0, 4).ToString()) + "%";
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            float num = Mathf.Clamp01((!_isProgress) ? _filled : (progress / 100f));
            Color color = bumpBehav.GetColor(colorEdge);
            _label.color = color;
            if (!isRectangular)
            {
                float num2 = rad - 15f + 8f * (bumpBehav.sizeBump + 0.5f * Mathf.Sin(bumpBehav.extraSizeBump * 3.1415927f)) * ((!held) ? 1f : (0.5f + 0.5f * Mathf.Sin(_pulse * 3.1415927f * 2f))) + 0.5f;
                for (int i = 0; i < _circles.Length; i++)
                {
                    _circles[i].scale = num2 / 8f;
                    _circles[i].SetPosition(rad, rad);
                }
                _circles[0].color = new Color(0.019607844f, 0f, Mathf.Lerp(0.3f, 0.6f, bumpBehav.col));
                _circles[1].color = color;
                _circles[1].alpha = 2f / num2;
                _circles[2].scale = (num2 + 10f) / 8f;
                _circles[2].alpha = num;
                _circles[2].color = Color.Lerp(Color.white, colorEdge, 0.7f);
                _circles[3].color = Color.Lerp(color, MenuColorEffect.MidToDark(color), 0.5f);
                _circles[3].scale = (num2 + 15f) / 8f;
                _circles[3].alpha = 2f / (num2 + 15f);
                float num3 = 0.5f + 0.5f * Mathf.Sin(bumpBehav.sin / 30f * 3.1415927f * 2f);
                num3 *= bumpBehav.sizeBump;
                if (greyedOut)
                {
                    num3 = 0f;
                }
                _circles[4].scale = (num2 - 8f * bumpBehav.sizeBump) / 8f;
                _circles[4].alpha = 2f / (num2 - 8f * bumpBehav.sizeBump);
                _circles[4].color = new Color(0f, 0f, num3);
                return;
            }
            if (greyedOut)
            {
                _rect.colorEdge = bumpBehav.GetColor(colorEdge);
                _rect.colorFill = bumpBehav.GetColor(colorFill);
                _rectH.colorEdge = bumpBehav.GetColor(colorEdge);
                _rect.GrafUpdate(timeStacker);
                _rectH.GrafUpdate(timeStacker);
                _fillSprite.scaleX = 0f;
                return;
            }
            _rectH.colorEdge = bumpBehav.GetColor(colorEdge);
            _rectH.addSize = new Vector2(-2f, -2f) * bumpBehav.AddSize;
            float alpha = ((Focused || MouseOver) && !held) ? ((0.5f + 0.5f * bumpBehav.Sin(10f)) * bumpBehav.AddSize) : 0f;
            for (int j = 0; j < 8; j++)
            {
                _rectH.sprites[j].alpha = alpha;
            }
            _rect.colorEdge = bumpBehav.GetColor(colorEdge);
            _rect.fillAlpha = bumpBehav.FillAlpha;
            _rect.addSize = 6f * bumpBehav.AddSize * Vector2.one;
            _rect.colorFill = colorFill;
            _rect.GrafUpdate(timeStacker);
            _rectH.GrafUpdate(timeStacker);
            if (num > 0f)
            {
                for (int k = 0; k < (Mathf.Approximately(num, 1f) ? 4 : 2); k++)
                {
                    _rect.sprites[4+k].alpha = 1f;
                    _rect.sprites[4+k].color = _rect.colorEdge;
                }
                _rect.sprites[0].alpha = 1f;
                _rect.sprites[0].color = _rect.colorEdge;
                if (Mathf.Approximately(num, 1f))
                {
                    _rect.sprites[2].alpha = 1f;
                    _rect.sprites[2].color = _rect.colorEdge;
                }
                _fillSprite.x = 6f - _rect.addSize.x / 2f;
                _fillSprite.y = 0f - _rect.addSize.y / 2f + 1.5f;
                _fillSprite.scaleX = (size.x - 14f + _rect.addSize.x) * num + 2.5f;
                _fillSprite.scaleY = size.y + _rect.addSize.y - 3f;
                _fillSprite.color = _rect.colorEdge;
                _rect.sprites[8].x = (size.x - 14f) * num + 7f;
                _rect.sprites[8].scaleX = (size.x - 14f) * (1f - num);
                _rect.sprites[1].x = (size.x - 14f) * num + 7f;
                _rect.sprites[1].scaleX = (size.x - 14f) * (1f - num);
                _rect.sprites[3].x = (size.x - 14f) * num + 7f;
                _rect.sprites[3].scaleX = (size.x - 14f) * (1f - num);
                _glow.size = new Vector2(Mathf.Min(size.x + 10f, _label.textRect.size.x * 1.5f), Mathf.Min(size.y + 10f, _label.textRect.size.y * 1.5f));
                _glow.centerPos = size / 2f;
                float t = Custom.SCurve(num, 0.6f);
                _glow.color = Color.Lerp(MenuColorEffect.rgbBlack, color, t);
                _glow.alpha = Mathf.Clamp01(1.5f - _fillSprite.scaleX / _glow.pos.x) * 0.6f;
                _glow.Show();
                _label.color = Color.Lerp(color, MenuColorEffect.MidToVeryDark(color), t);
                return;
            }
            _fillSprite.scaleX = 0f;
            _glow.Hide();
        }

        public override void Update()
        {
            if (greyedOut && held)
            {
                held = false;
            }
            base.Update();
            if (isRectangular)
            {
                _rect.Update();
                _rectH.Update();
            }
            if (greyedOut || _isProgress)
            {
                _filled = 0f;
                _pulse = 0f;
                _hasSignalled = false;
                bumpBehav.sizeBump = (greyedOut ? 0f : 1f);
                if (greyedOut)
                {
                    bumpBehav.sin = 0f;
                    return;
                }
            }
            if (held && !_isProgress)
            {
                _pulse += frameMulti * _filled / 20f;
            }
            else
            {
                _pulse = 0f;
            }
            bool oldHeld = held;
            if (MenuMouseMode)
            {
                if (held)
                {
                    held = Input.GetMouseButton(0);
                }
                else
                {
                    held = (MouseOver && Input.GetMouseButton(0));
                }
            }
            else
            {
                held = (held && CtlrInput.jmp);
            }
            bumpBehav.sizeBump = ((!held) ? 0f : 1f);
            if (!held)
            {
                if (oldHeld)
                {
                    OnClick?.Invoke(this);
                }
                if (oldHeld && !_hasSignalled && !_isProgress)
                {
                    PlaySound(SoundID.MENU_Security_Button_Release);
                }
                if (_hasSignalled)
                {
                    _releaseCounter++;
                    if (_releaseCounter <= ModdingMenu.DASinit * 1.5f)
                    {
                        _filled = 1f;
                        return;
                    }
                    _filled = Custom.LerpAndTick(_filled, 0f, 0.04f, 0.025f * frameMulti);
                    if (_filled < 0.5f)
                    {
                        _hasSignalled = false;
                        return;
                    }
                }
                else
                {
                    _filled = Custom.LerpAndTick(_filled, 0f, 0.04f, 0.025f * frameMulti);
                }
                return;
            }
            if (!oldHeld)
            {
                OnPressInit?.Invoke(this);
            }
            if (_isProgress)
            {
                return;
            }
            bumpBehav.sin = _pulse;
            _filled = Custom.LerpAndTick(_filled, 1f, 0.007f, frameMulti / _fillTime);
            if (_filled >= 1f && !_hasSignalled)
            {
                if (OnPressDone != null)
                {
                    OnPressDone(this);
                    Menu.PlaySound(SoundID.MENU_Security_Button_Release);
                }
                _hasSignalled = true;
            }
            _releaseCounter = 0;
        }

        public void SetProgress(float percentage)
        {
            if (percentage < 0f)
            {
                _isProgress = false;
                progress = 0f;
                return;
            }
            if (!_isProgress)
            {
                _isProgress = true;
                text = "";
            }
            progress = Mathf.Clamp(percentage, 0f, 100f);
            Change();
        }

        protected override void Deactivate()
        {
            base.Deactivate();
            _filled = 0f;
            _pulse = 0f;
            _hasSignalled = false;
        }

        protected override void Freeze()
        {
            base.Freeze();
        }
    }
}
