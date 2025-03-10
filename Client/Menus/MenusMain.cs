﻿using GTA;

using System.Drawing;

using LemonUI;
using LemonUI.Menus;

namespace CoopClient.Menus
{
    /// <summary>
    /// Don't use it!
    /// </summary>
    public class MenusMain
    {
        internal ObjectPool MenuPool = new ObjectPool();

        internal NativeMenu MainMenu = new NativeMenu("RAGECOOP", "MAIN")
        {
            UseMouse = false,
            Alignment = Main.MainSettings.FlipMenu ? GTA.UI.Alignment.Right : GTA.UI.Alignment.Left
        };
        #region SUB
        internal Sub.Settings SubSettings = new Sub.Settings();
        internal Sub.Servers ServerList = new Sub.Servers();
        #endregion

        #region ITEMS
        private readonly NativeItem _usernameItem = new NativeItem("Username") { AltTitle = Main.MainSettings.Username };
        internal readonly NativeItem ServerIpItem = new NativeItem("Server IP") { AltTitle = Main.MainSettings.LastServerAddress };
        private readonly NativeItem _serverConnectItem = new NativeItem("Connect");
        private readonly NativeItem _aboutItem = new NativeItem("About", "~y~SOURCE~s~~n~" +
            "https://github.com/RAGECOOP~n~" +
            "~y~VERSION~s~~n~" +
            Main.CurrentVersion.Replace("_", ".")) { LeftBadge = new LemonUI.Elements.ScaledTexture("commonmenu", "shop_new_star") };
        #endregion

        /// <summary>
        /// Don't use it!
        /// </summary>
        public MenusMain()
        {
            MainMenu.Banner.Color = Color.FromArgb(225, 0, 0, 0);
            MainMenu.Title.Color = Color.FromArgb(255, 165, 0);

            _usernameItem.Activated += UsernameActivated;
            ServerIpItem.Activated += ServerIpActivated;
            _serverConnectItem.Activated += (sender, item) => { Main.MainNetworking.DisConnectFromServer(Main.MainSettings.LastServerAddress); };

            MainMenu.AddSubMenu(ServerList.MainMenu);

            MainMenu.Add(_usernameItem);
            MainMenu.Add(ServerIpItem);
            MainMenu.Add(_serverConnectItem);

            MainMenu.AddSubMenu(SubSettings.MainMenu);

            MainMenu.Add(_aboutItem);

            MenuPool.Add(ServerList.MainMenu);
            MenuPool.Add(MainMenu);
            MenuPool.Add(SubSettings.MainMenu);
        }

        internal void UsernameActivated(object a, System.EventArgs b)
        {
            string newUsername = Game.GetUserInput(WindowTitle.EnterMessage20, _usernameItem.AltTitle, 20);
            if (!string.IsNullOrWhiteSpace(newUsername))
            {
                Main.MainSettings.Username = newUsername;
                Util.SaveSettings();

                _usernameItem.AltTitle = newUsername;
            }
        }

        internal void ServerIpActivated(object a, System.EventArgs b)
        {
            string newServerIp = Game.GetUserInput(WindowTitle.EnterMessage60, ServerIpItem.AltTitle, 60);
            if (!string.IsNullOrWhiteSpace(newServerIp) && newServerIp.Contains(":"))
            {
                Main.MainSettings.LastServerAddress = newServerIp;
                Util.SaveSettings();

                ServerIpItem.AltTitle = newServerIp;
            }
        }

        internal void InitiateConnectionMenuSetting()
        {
            MainMenu.Items[0].Enabled = false;
            MainMenu.Items[1].Enabled = false;
            MainMenu.Items[2].Enabled = false;
            MainMenu.Items[3].Enabled = false;
        }

        internal void ConnectedMenuSetting()
        {
            MainMenu.Items[3].Enabled = true;
            MainMenu.Items[3].Title = "Disconnect";
            SubSettings.MainMenu.Items[1].Enabled = !Main.DisableTraffic && Main.NPCsAllowed;

            MainMenu.Visible = false;
            ServerList.MainMenu.Visible = false;
        }

        internal void DisconnectedMenuSetting()
        {
            MainMenu.Items[0].Enabled = true;
            MainMenu.Items[1].Enabled = true;
            MainMenu.Items[2].Enabled = true;
            MainMenu.Items[3].Enabled = true;
            MainMenu.Items[3].Title = "Connect";
            SubSettings.MainMenu.Items[1].Enabled = false;
        }
    }
}
