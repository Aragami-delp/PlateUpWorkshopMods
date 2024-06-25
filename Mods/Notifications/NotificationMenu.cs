using Controllers;
using Kitchen;
using Kitchen.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Notifications
{
    public class NotificationMenu : Menu<PauseMenuAction>
    {
        private LabelElement m_notificationLabelElement;
        private InfoBoxElement m_notificationTextElement;

        public NotificationMenu(Transform container, ModuleList module_list) : base(container, module_list)
        {
            string t = container != null ? Container.name : "nothing";
            Notifications.LogError($"3 {t}");
        }

        public override void Setup(int player_id)
        {
            m_notificationLabelElement = AddLabel("_modName");
            m_notificationTextElement = AddInfo("_notificationText");
        }

        public void SetNotification(string _label, string _text)
        {
            m_notificationLabelElement.SetLabel(_label);
            m_notificationTextElement.SetLabel(_text);
        }
    }
}
