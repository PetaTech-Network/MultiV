﻿using System;
using System.Net;
using System.Drawing;
using System.Collections.Generic;

using Newtonsoft.Json;

using LemonUI.Menus;

namespace CoopClient.Menus.Sub
{
    internal class ServerListClass
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("port")]
        public string Port { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("players")]
        public int Players { get; set; }
        [JsonProperty("maxPlayers")]
        public int MaxPlayers { get; set; }
        [JsonProperty("allowlist")]
        public bool AllowList { get; set; }
        [JsonProperty("mods")]
        public bool Mods { get; set; }
        [JsonProperty("npcs")]
        public bool NPCs { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
    }

    /// <summary>
    /// Don't use it!
    /// </summary>
    internal class Servers
    {
        internal NativeMenu MainMenu = new NativeMenu("RAGECOOP", "Servers", "Go to the server list")
        {
            UseMouse = false,
            Alignment = Main.MainSettings.FlipMenu ? GTA.UI.Alignment.Right : GTA.UI.Alignment.Left
        };
        internal NativeItem ResultItem = null;

        /// <summary>
        /// Don't use it!
        /// </summary>
        public Servers()
        {
            MainMenu.Banner.Color = Color.FromArgb(225, 0, 0, 0);
            MainMenu.Title.Color = Color.FromArgb(255, 165, 0);

            MainMenu.Opening += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                MainMenu.Add(ResultItem = new NativeItem("Loading..."));
                GetAllServer();
            };
            MainMenu.Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                CleanUpList();
            };
        }

        private void CleanUpList()
        {
            MainMenu.Clear();
            ResultItem = null;
        }

        private void GetAllServer()
        {
            List<ServerListClass> serverList = null;
            try
            {
                // TLS only
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                WebClient client = new WebClient();
                string data = client.DownloadString(Main.MainSettings.MasterServer);
                serverList = JsonConvert.DeserializeObject<List<ServerListClass>>(data);
            }
            catch (Exception ex)
            {
                ResultItem.Title = "Download failed!";
                ResultItem.Description = ex.Message;
                return;
            }
            
            if (serverList == null)
            {
                ResultItem.Title = "Something went wrong!";
                return;
            }
            if (serverList.Count == 0)
            {
                ResultItem.Title = "No server was found!";
                return;
            }

            CleanUpList();

            foreach (ServerListClass server in serverList)
            {
                string address = $"{server.Address}:{server.Port}";
                NativeItem tmpItem = new NativeItem($"[{server.Country}] {server.Name}", $"~b~{address}~s~~n~~g~Version {server.Version}.x~s~~n~Mods = {server.Mods}~n~NPCs = {server.NPCs}") { AltTitle = $"[{server.Players}/{server.MaxPlayers}][{(server.AllowList ? "~r~X~s~" : "~g~O~s~")}]" };
                tmpItem.Activated += (object sender, EventArgs e) =>
                {
                    try
                    {
                        MainMenu.Visible = false;

                        Main.MainNetworking.DisConnectFromServer(address);
#if !NON_INTERACTIVE
                        Main.MainMenu.ServerIpItem.AltTitle = address;

                        Main.MainMenu.MainMenu.Visible = true;
#endif
                        Main.MainSettings.LastServerAddress = address;
                        Util.SaveSettings();
                    }
                    catch (Exception ex)
                    {
                        GTA.UI.Notification.Show($"~r~{ex.Message}");
                    }
                };
                MainMenu.Add(tmpItem);
            }
        }
    }
}
