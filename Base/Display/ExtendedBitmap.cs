﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Base.Display
{

    public class ExtendedBitmap : IDisposable, ICloneable
    {

    
        public Graphics Graphics
        {
            get
            {
                Graphics graphics;
                if (this._isInitialised)
                {
                    this._isNew = false;
                    graphics = this._surface;
                }
                else if (this.Initialise())
                {
                    this._isNew = false;
                    graphics = this._surface;
                }
                else
                {
                    graphics = null;
                }
                return graphics;
            }
        }


    
        public Bitmap Bitmap
        {
            get
            {
                Bitmap result;
                if (this._isInitialised)
                {
                    result = this._bits;
                }
                else if (!this.Initialise())
                {
                    result = null;
                }
                else
                {
                    result = this._bits;
                }
                return result;
            }
        }


    
        bool CanInitialise
        {
            get
            {
                bool flag;
                if (this._isDisposed)
                {
                    flag = false;
                }
                else if (this.Cache.Size.Width > 0 & this.Cache.Size.Height > 0)
                {
                    flag = true;
                }
                else if (this.Cache.Bounds.Width > 0 & this.Cache.Bounds.Height > 0)
                {
                    this.Cache.Size.Width = this.Cache.Bounds.Width;
                    this.Cache.Size.Height = this.Cache.Bounds.Height;
                    flag = true;
                }
                else
                {
                    flag = false;
                }
                return flag;
            }
        }


    
    
        public Size Size
        {
            get
            {
                Size result;
                if (!this._isInitialised)
                {
                    result = default(Size);
                }
                else
                {
                    result = this.Cache.Size;
                }
                return result;
            }
            set
            {
                if (value.Width != this.Cache.Size.Width || value.Height != this.Cache.Size.Height)
                {
                    this.Cache.Size = value;
                    this.Initialise();
                }
            }
        }


    
    
        Region Clip
        {
            get
            {
                Region result;
                if (!this._isInitialised)
                {
                    result = new Region();
                }
                else
                {
                    result = this.Cache.Clip;
                }
                return result;
            }
            set
            {
                if (this._isInitialised)
                {
                    this._surface.Clip = value;
                    this.Cache.Update(ref this._surface);
                    this._isNew = false;
                }
            }
        }


    
        public Rectangle ClipRect
        {
            get
            {
                Rectangle result;
                if (!this._isInitialised)
                {
                    result = default(Rectangle);
                }
                else
                {
                    result = this.Cache.ClipRect;
                }
                return result;
            }
        }


        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this._isDisposed)
                {
                    this._isNew = false;
                    this._isInitialised = false;
                    if (this._surface != null)
                    {
                        this._surface.Dispose();
                    }
                    if (this._bits != null)
                    {
                        this._bits.Dispose();
                    }
                    if (this.Cache.Clip != null)
                    {
                        this.Cache.Clip.Dispose();
                    }
                    this._isDisposed = true;
                }
            }
        }


        public object Clone()
        {
            object obj;
            if (!this._isInitialised)
            {
                obj = new ExtendedBitmap();
            }
            else
            {
                ExtendedBitmap extendedBitmap = new ExtendedBitmap(this.Size)
                {
                    Cache = this.Cache
                };
                extendedBitmap._surface.DrawImageUnscaled(this._bits, new Point(0, 0));
                extendedBitmap.Clip = this.Clip;
                extendedBitmap._isInitialised = this._isInitialised;
                extendedBitmap._isNew = this._isNew;
                obj = extendedBitmap;
            }
            return obj;
        }


        bool Initialise()
        {
            bool flag;
            if (!this.CanInitialise)
            {
                flag = false;
            }
            else
            {
                if (this._surface != null)
                {
                    this._surface.Dispose();
                }
                if (this._bits != null)
                {
                    this._bits.Dispose();
                }
                this._bits = new Bitmap(this.Cache.Size.Width, this.Cache.Size.Height, this.Cache.BitDepth);
                this._surface = Graphics.FromImage(this._bits);
                this.Cache.Update(ref this._bits);
                this._surface.Clip = new Region(this.Cache.Bounds);
                this.Cache.Update(ref this._surface);
                this._isNew = true;
                this._isInitialised = true;
                flag = true;
            }
            return flag;
        }


        void Initialise(string fileName)
        {
            if (this._surface != null)
            {
                this._surface.Dispose();
            }
            if (this._bits != null)
            {
                this._bits.Dispose();
            }
            if (!File.Exists(fileName))
            {
                this.Cache = new ExtendedBitmap.PropertyCache
                {
                    Size = new Size(32, 32)
                };
                this.Initialise();
            }
            else
            {
                this._bits = new Bitmap(fileName);
                this._surface = Graphics.FromImage(this._bits);
                this.Cache.Update(ref this._bits);
                this._surface.Clip = new Region(this.Cache.Bounds);
                this.Cache.Update(ref this._surface);
                this._isNew = true;
                this._isInitialised = true;
            }
        }


        ExtendedBitmap()
        {
            this.Cache = new ExtendedBitmap.PropertyCache();
            this._isNew = true;
            this._isInitialised = false;
        }


        public ExtendedBitmap(Size imageSize)
        {
            this.Cache = new ExtendedBitmap.PropertyCache
            {
                Size = imageSize
            };
            this.Initialise();
        }


        public ExtendedBitmap(int x, int y)
        {
            this.Cache = new ExtendedBitmap.PropertyCache
            {
                Size = new Size(x, y)
            };
            this.Initialise();
        }


        public ExtendedBitmap(string fileName)
        {
            this.Cache = new ExtendedBitmap.PropertyCache();
            this.Initialise(fileName);
        }


        bool _isDisposed;


        Bitmap _bits;


        Graphics _surface;


        protected ExtendedBitmap.PropertyCache Cache;


        bool _isNew;


        bool _isInitialised;


        protected class PropertyCache
        {

            public void Update(ref Bitmap args)
            {
                this.Size = args.Size;
                this._location = new Point(0, 0);
                this.Bounds = new Rectangle(this._location, this.Size);
                this.BitDepth = args.PixelFormat;
            }


            public void Update(ref Graphics args)
            {
                if (this.Clip != null)
                {
                    this.Clip.Dispose();
                }
                this.Clip = args.Clip;
                this.ClipRect = ExtendedBitmap.PropertyCache.RectConvert(args.ClipBounds);
            }


            static Rectangle RectConvert(RectangleF iRect)
            {
                int x;
                if (iRect.X > 2.14748365E+09f)
                {
                    x = int.MaxValue;
                }
                else if (iRect.X < -2.14748365E+09f)
                {
                    x = int.MinValue;
                }
                else
                {
                    x = Convert.ToInt32(iRect.X);
                }
                int y;
                if (iRect.Y > 2.14748365E+09f)
                {
                    y = int.MaxValue;
                }
                else if (iRect.Y < -2.14748365E+09f)
                {
                    y = int.MinValue;
                }
                else
                {
                    y = Convert.ToInt32(iRect.Y);
                }
                int width;
                if (iRect.Width > 2.14748365E+09f)
                {
                    width = int.MaxValue;
                }
                else if (iRect.Width < -2.14748365E+09f)
                {
                    width = int.MinValue;
                }
                else
                {
                    width = Convert.ToInt32(iRect.Width);
                }
                int height;
                if (iRect.Height > 2.14748365E+09f)
                {
                    height = int.MaxValue;
                }
                else if (iRect.Height < -2.14748365E+09f)
                {
                    height = int.MinValue;
                }
                else
                {
                    height = Convert.ToInt32(iRect.Height);
                }
                return new Rectangle(x, y, width, height);
            }


            public Size Size;


            Point _location;


            public Rectangle Bounds;


            public Region Clip;


            public Rectangle ClipRect;


            public PixelFormat BitDepth = PixelFormat.Format32bppArgb;
        }
    }
}
