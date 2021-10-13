/*
 * Copyright © 2008-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System.Collections.Generic;

    internal class ViewState
    {
        private Stack<ViewData> backData = new Stack<ViewData>();
        private Stack<ViewData> fwdData = new Stack<ViewData>();

        public delegate void UpdateNavBtnStateEventHandler(bool enableBack, bool enableFwd);

        public delegate void ViewChangedEventHandler(View view, MainTab tab, object data);

        public event UpdateNavBtnStateEventHandler UpdateNavBtnState;

        public event ViewChangedEventHandler ViewChanged;

        internal enum MainTab
        {
            FindProgramme,
            Favourites,
            Subscriptions,
            Downloads,
        }

        internal enum View
        {
            FindNewChooseProvider,
            FindNewProviderForm,
            ProgEpisodes,
            Favourites,
            Subscriptions,
            Downloads,
        }

        public View CurrentView
        {
            get { return this.backData.Peek().View; }
        }

        public object CurrentViewData
        {
            get
            {
                return this.backData.Peek().Data;
            }

            set
            {
                ViewData curView = this.backData.Peek();
                curView.Data = value;
            }
        }

        public void SetView(MainTab tab, View view)
        {
            this.SetView(tab, view, null);
        }

        public void SetView(MainTab tab, View view, object viewData)
        {
            this.StoreView(tab, view, viewData);
            this.ViewChanged?.Invoke(view, tab, viewData);
        }

        public void SetView(View view, object viewData)
        {
            ViewData currentView = this.backData.Peek();
            this.SetView(currentView.Tab, view, viewData);
        }

        public void StoreView(MainTab tab, View view, object viewData)
        {
            ViewData storeView = default(ViewData);

            storeView.Tab = tab;
            storeView.View = view;
            storeView.Data = viewData;

            this.backData.Push(storeView);

            if (this.fwdData.Count > 0)
            {
                this.fwdData.Clear();
            }

            this.UpdateNavBtnState?.Invoke(this.backData.Count > 1, false);
        }

        public void StoreView(View view, object viewData)
        {
            ViewData currentView = this.backData.Peek();
            this.StoreView(currentView.Tab, view, viewData);
        }

        public void StoreView(object viewData)
        {
            ViewData currentView = this.backData.Peek();
            this.StoreView(currentView.Tab, currentView.View, viewData);
        }

        public void NavBack()
        {
            this.fwdData.Push(this.backData.Pop());
            ViewData curView = this.backData.Peek();

            this.UpdateNavBtnState?.Invoke(this.backData.Count > 1, this.fwdData.Count > 0);
            this.ViewChanged?.Invoke(curView.View, curView.Tab, curView.Data);
        }

        public void NavFwd()
        {
            this.backData.Push(this.fwdData.Pop());
            ViewData curView = this.backData.Peek();

            this.UpdateNavBtnState?.Invoke(this.backData.Count > 1, this.fwdData.Count > 0);
            this.ViewChanged?.Invoke(curView.View, curView.Tab, curView.Data);
        }

        private struct ViewData
        {
            public MainTab Tab;
            public View View;
            public object Data;
        }
    }
}
