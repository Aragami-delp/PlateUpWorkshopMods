using Controllers;
using Kitchen;
using Kitchen.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Notifications
{
    public class NotificationView : LocalMenuView<PauseMenuAction>
    {
        //protected override bool LowPriorityInputConsumer => false; // Should not consume any input

        protected override void SetupMenus()
        {
            Notifications.LogError("SetupMenus0");
            AddMenu(typeof(NotificationMenu), new NotificationMenu(ButtonContainer, ModuleList));
            SetMenu(typeof(NotificationMenu));
            Notifications.LogError("SetupMenus1");
        }

        public override void Hide()
        {
            if (IsDismissed)
                return;
            IsDismissed = true;
            Container.SetActive(false);
            ActivePlayer = 0;
        }

        public override InputConsumerState TakeInput(int player_id, InputState state) // Called by input and therefor prob not used - have to create notification myself
        {
            SetPlayer(player_id);
            return InputConsumerState.NotConsumed; // Should not consume any input
        }

        protected override void PerformAction(PauseMenuAction action = default)
        {
            return; // As it doesnt not handle any input action
        }

        protected override void CreateForPlayer(int player_id)
        {
            if (Panel == null)
            {
                Notifications.LogError("Add");
                Panel = ModuleDirectory.Add<PanelElement>(ButtonContainer);
            }
            //ActivePlayer = player;
            SetPlayer(player_id);
            IsDismissed = false;
            Menus.Clear();
            ActiveMenuStack.Clear();
            SetupMenus();
            Container.SetActive(true);
        }

        /// <summary>
        /// Shows a new notification as soon as possible
        /// </summary>
        /// <param name="_label"></param>
        /// <param name="_text"></param>
        public void ShowNewNotification(string _label = "_label", string _text = "_text", float _time = 3f, int _player_id = 0)
        {
            CreateForPlayer(0);
            (Menus[typeof(NotificationMenu)] as NotificationMenu).SetNotification(_label, _text);
            StartCoroutine(HideNotification(_time));
        }

        private IEnumerator HideNotification(float _time)
        {
            yield return new WaitForSeconds(_time);
            Hide();
        }

        public override void Start()
        {
            //DontDestroyOnLoad(this);
            return;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                ShowNewNotification("Hallo", "Inhalt hier hin", 3f, 1);
            }
        }

        public override void OnDestroy()
        {
            return;
        }
    }
}
