using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility
{
	/*
	 * Source: https://github.com/mdymel/superfastblur
	 * License: 
	 * 
	 * The MIT License (MIT)

	 * Copyright (c) 2015 Michal Dymel
	 * 
	 * Permission is hereby granted, free of charge, to any person obtaining a copy
	 * of this software and associated documentation files (the "Software"), to deal
	 * in the Software without restriction, including without limitation the rights
	 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	 * copies of the Software, and to permit persons to whom the Software is
	 * furnished to do so, subject to the following conditions:
	 * 
	 * The above copyright notice and this permission notice shall be included in all
	 * copies or substantial portions of the Software.
	 * 
	 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	 * SOFTWARE.
	 * 
	 */

	public class GaussianBlur
	{
		private readonly int[] _red;
		private readonly int[] _green;
		private readonly int[] _blue;

		private readonly int _width;
		private readonly int _height;

		private readonly ParallelOptions _pOptions = new ParallelOptions { MaxDegreeOfParallelism = 16 };

		public GaussianBlur(Bitmap image)
		{
			var rct = new Rectangle(0, 0, image.Width, image.Height);
			var source = new int[rct.Width * rct.Height];
			var bits = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			Marshal.Copy(bits.Scan0, source, 0, source.Length);
			image.UnlockBits(bits);

			_width = image.Width;
			_height = image.Height;

			_red = new int[_width * _height];
			_green = new int[_width * _height];
			_blue = new int[_width * _height];

			Parallel.For(0, source.Length, _pOptions, i =>
			{
				_red[i] = (source[i] & 0xff0000) >> 16;
				_green[i] = (source[i] & 0x00ff00) >> 8;
				_blue[i] = (source[i] & 0x0000ff);
			});
		}

		public Bitmap Process(int radial)
		{
			var newRed = new int[_width * _height];
			var newGreen = new int[_width * _height];
			var newBlue = new int[_width * _height];
			var dest = new int[_width * _height];

			Parallel.Invoke(
				() => gaussBlur_4(_red, newRed, radial),
				() => gaussBlur_4(_green, newGreen, radial),
				() => gaussBlur_4(_blue, newBlue, radial));

			Parallel.For(0, dest.Length, _pOptions, i =>
			{
				if(newRed[i] > 255) newRed[i] = 255;
				if(newGreen[i] > 255) newGreen[i] = 255;
				if(newBlue[i] > 255) newBlue[i] = 255;

				if(newRed[i] < 0) newRed[i] = 0;
				if(newGreen[i] < 0) newGreen[i] = 0;
				if(newBlue[i] < 0) newBlue[i] = 0;

				dest[i] = (int)(0xff000000u | (uint)(newRed[i] << 16) | (uint)(newGreen[i] << 8) | (uint)newBlue[i]);
			});

			var image = new Bitmap(_width, _height);
			var rct = new Rectangle(0, 0, image.Width, image.Height);
			var bits2 = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			Marshal.Copy(dest, 0, bits2.Scan0, dest.Length);
			image.UnlockBits(bits2);
			return image;
		}

		private void gaussBlur_4(int[] source, int[] dest, int r)
		{
			var bxs = boxesForGauss(r, 3);
			boxBlur_4(source, dest, _width, _height, (bxs[0] - 1) / 2);
			boxBlur_4(dest, source, _width, _height, (bxs[1] - 1) / 2);
			boxBlur_4(source, dest, _width, _height, (bxs[2] - 1) / 2);
		}

		private int[] boxesForGauss(int sigma, int n)
		{
			var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
			var wl = (int)Math.Floor(wIdeal);
			if(wl % 2 == 0) wl--;
			var wu = wl + 2;

			var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
			var m = Math.Round(mIdeal);

			var sizes = new List<int>();
			for(var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);
			return sizes.ToArray();
		}

		private void boxBlur_4(int[] source, int[] dest, int w, int h, int r)
		{
			for(var i = 0; i < source.Length; i++) dest[i] = source[i];
			boxBlurH_4(dest, source, w, h, r);
			boxBlurT_4(source, dest, w, h, r);
		}

		private void boxBlurH_4(int[] source, int[] dest, int w, int h, int r)
		{
			var iar = (double)1 / (r + r + 1);
			Parallel.For(0, h, _pOptions, i =>
			{
				var ti = i * w;
				var li = ti;
				var ri = ti + r;
				var fv = source[ti];
				var lv = source[ti + w - 1];
				var val = (r + 1) * fv;
				for(var j = 0; j < r; j++) val += source[ti + j];
				for(var j = 0; j <= r; j++)
				{
					val += source[ri++] - fv;
					dest[ti++] = (int)Math.Round(val * iar);
				}
				for(var j = r + 1; j < w - r; j++)
				{
					val += source[ri++] - dest[li++];
					dest[ti++] = (int)Math.Round(val * iar);
				}
				for(var j = w - r; j < w; j++)
				{
					val += lv - source[li++];
					dest[ti++] = (int)Math.Round(val * iar);
				}
			});
		}

		private void boxBlurT_4(int[] source, int[] dest, int w, int h, int r)
		{
			var iar = (double)1 / (r + r + 1);
			Parallel.For(0, w, _pOptions, i =>
			{
				var ti = i;
				var li = ti;
				var ri = ti + r * w;
				var fv = source[ti];
				var lv = source[ti + w * (h - 1)];
				var val = (r + 1) * fv;
				for(var j = 0; j < r; j++) val += source[ti + j * w];
				for(var j = 0; j <= r; j++)
				{
					val += source[ri] - fv;
					dest[ti] = (int)Math.Round(val * iar);
					ri += w;
					ti += w;
				}
				for(var j = r + 1; j < h - r; j++)
				{
					val += source[ri] - source[li];
					dest[ti] = (int)Math.Round(val * iar);
					li += w;
					ri += w;
					ti += w;
				}
				for(var j = h - r; j < h; j++)
				{
					val += lv - source[li];
					dest[ti] = (int)Math.Round(val * iar);
					li += w;
					ti += w;
				}
			});
		}
	}
}
