using Controllers;
using Kitchen;
using Kitchen.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderUI
{
    public class LocalNotificationView : LocalMenuView<PauseMenuAction>
    {
        //protected override bool LowPriorityInputConsumer => false; // Should not consume any input

        protected override void SetupMenus()
        {
            AddMenu(typeof(LocalNotificationMenu), (Menu<PauseMenuAction>) new LocalNotificationMenu(this.ButtonContainer, this.ModuleList));
            SetMenu(typeof(LocalNotificationMenu));
        }

        public override void Hide()
        {
            if (IsDismissed)
                return;
            IsDismissed = true;
            Container.SetActive(false);
            ActivePlayer = 0;
        }

        public override InputConsumerState TakeInput(int player_id, InputState state)
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
                Panel = ModuleDirectory.Add<PanelElement>(ButtonContainer);
            //ActivePlayer = player;
            SetPlayer(player_id);
            IsDismissed = false;
            Menus.Clear();
            ActiveMenuStack.Clear();
            SetupMenus();
            Container.SetActive(true);
        }

        public override void Start()
        {
            DontDestroyOnLoad(this);
            return;
        }

        public override void OnDestroy()
        {
            return;
        }
    }
}
