using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
// Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.


using System.Collections.Generic;
namespace RadioDld
{

	internal class ViewState
	{
		public enum MainTab
		{
			FindProgramme,
			Favourites,
			Subscriptions,
			Downloads
		}

		public enum View
		{
			FindNewChooseProvider,
			FindNewProviderForm,
			ProgEpisodes,
			Favourites,
			Subscriptions,
			Downloads
		}

		private struct ViewData
		{
			public MainTab Tab;
			public View View;
			public object Data;
		}

		private Stack<ViewData> backData = new Stack<ViewData>();

		private Stack<ViewData> fwdData = new Stack<ViewData>();
		public event UpdateNavBtnStateEventHandler UpdateNavBtnState;
		public delegate void UpdateNavBtnStateEventHandler(bool enableBack, bool enableFwd);
		public event ViewChangedEventHandler ViewChanged;
		public delegate void ViewChangedEventHandler(View view, MainTab tab, object data);

		public View CurrentView {
			get { return backData.Peek().View; }
		}

		public object CurrentViewData {
			get { return backData.Peek().Data; }
			set {
				ViewData curView = backData.Peek();
				curView.Data = value;
			}
		}

		public void SetView(MainTab tab, View view)
		{
			SetView(tab, view, null);
		}

		public void SetView(MainTab tab, View view, object viewData)
		{
			StoreView(tab, view, viewData);
			if (ViewChanged != null) {
				ViewChanged(view, tab, viewData);
			}
		}

		public void SetView(View view, object viewData)
		{
			ViewData currentView = backData.Peek();
			SetView(currentView.Tab, view, viewData);
		}

		public void StoreView(MainTab tab, View view, object viewData)
		{
			ViewData storeView = default(ViewData);

			storeView.Tab = tab;
			storeView.View = view;
			storeView.Data = viewData;

			backData.Push(storeView);

			if (fwdData.Count > 0) {
				fwdData.Clear();
			}

			if (UpdateNavBtnState != null) {
				UpdateNavBtnState(backData.Count > 1, false);
			}
		}

		public void StoreView(View view, object viewData)
		{
			ViewData currentView = backData.Peek();
			StoreView(currentView.Tab, view, viewData);
		}

		public void StoreView(object viewData)
		{
			ViewData currentView = backData.Peek();
			StoreView(currentView.Tab, currentView.View, viewData);
		}

		public void NavBack()
		{
			fwdData.Push(backData.Pop());

			ViewData curView = backData.Peek();

			if (UpdateNavBtnState != null) {
				UpdateNavBtnState(backData.Count > 1, fwdData.Count > 0);
			}
			if (ViewChanged != null) {
				ViewChanged(curView.View, curView.Tab, curView.Data);
			}
		}

		public void NavFwd()
		{
			backData.Push(fwdData.Pop());

			ViewData curView = backData.Peek();

			if (UpdateNavBtnState != null) {
				UpdateNavBtnState(backData.Count > 1, fwdData.Count > 0);
			}
			if (ViewChanged != null) {
				ViewChanged(curView.View, curView.Tab, curView.Data);
			}
		}
	}
}
