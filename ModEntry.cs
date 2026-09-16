using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

using Color = Microsoft.Xna.Framework.Color;

namespace SaveFolderShortcut
{
    public class ModEntry : Mod
    {
        private FileSystemWatcher watcher;
        private bool hasNewSaves = false;
        private HashSet<string> knownSaves = new HashSet<string>();
        
        private ClickableTextureComponent openFolderButton;
        private bool isButtonHovered = false;
        private bool buttonInitialized = false;
        
        public override void Entry(IModHelper helper)
        {
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            
            Monitor.Log("Mod loaded successfully!", LogLevel.Info);
            SetupFileSystemWatcher();
        }
        
        private void SetupFileSystemWatcher()
        {
            try
            {
                string savesPath = Constants.SavesPath;
                
                if (!Directory.Exists(savesPath))
                {
                    Monitor.Log($"Saves folder not found: {savesPath}", LogLevel.Warn);
                    return;
                }
                
                foreach (var dir in Directory.GetDirectories(savesPath))
                {
                    knownSaves.Add(Path.GetFileName(dir));
                }
                
                watcher = new FileSystemWatcher(savesPath)
                {
                    NotifyFilter = NotifyFilters.DirectoryName,
                    EnableRaisingEvents = true
                };
                
                watcher.Created += OnSaveCreated;
                watcher.Deleted += OnSaveDeleted;
                
                Monitor.Log($"Watching saves folder: {savesPath}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to setup file watcher: {ex.Message}", LogLevel.Error);
            }
        }
        
        private void OnSaveCreated(object sender, FileSystemEventArgs e)
        {
            if (!knownSaves.Contains(e.Name))
            {
                hasNewSaves = true;
                Monitor.Log($"New save detected: {e.Name}", LogLevel.Info);
            }
        }
        
        private void OnSaveDeleted(object sender, FileSystemEventArgs e)
        {
            knownSaves.Remove(e.Name);
        }
        
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            Monitor.Log($"Menu changed: OldMenu={e.OldMenu?.GetType().Name ?? "null"}, NewMenu={e.NewMenu?.GetType().Name ?? "null"}", LogLevel.Debug);
            
            if (e.NewMenu is TitleMenu)
            {
                Monitor.Log("TitleMenu detected! Initializing button...", LogLevel.Info);
                InitializeOpenFolderButton();
                buttonInitialized = true;
                
                
            }
        }
        
        private void InitializeOpenFolderButton()
        {
            int x = 20;
            int y = 980;
            int width = 395;
            int height = 70;
            
            openFolderButton = new ClickableTextureComponent(
                "OpenSavesFolder",
                new Rectangle(x, y, width, height),
                null,
                Helper.Translation.Get("open_saves_folder"),
                Game1.mouseCursors,
                new Rectangle(365, 494, 12, 11),
                4f)
            {
                myID = 999
            };
            
            Monitor.Log($"Button created: bounds={openFolderButton.bounds}", LogLevel.Info);
        }
        
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            bool isTitleMenu = Game1.activeClickableMenu is TitleMenu;
            
            if (isTitleMenu && openFolderButton != null)
            {
                DrawOpenFolderButton(e.SpriteBatch);
            }
        }
        
        private void DrawOpenFolderButton(SpriteBatch b)
{
    if (openFolderButton == null) return;
    
    isButtonHovered = openFolderButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY());
    
    Color bgColor = isButtonHovered ? new Color(200, 220, 255) : Color.White;
    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18),
        openFolderButton.bounds.X, openFolderButton.bounds.Y,
        openFolderButton.bounds.Width, openFolderButton.bounds.Height,
        bgColor, 4f, true);
    
    Rectangle sourceRect = new Rectangle(365, 494, 12, 11);
    b.Draw(Game1.mouseCursors,
        new Vector2(openFolderButton.bounds.X + 15, openFolderButton.bounds.Y + 15),
        sourceRect,
        isButtonHovered ? Color.DarkBlue : Color.SaddleBrown,
        0f,
        Vector2.Zero,
        4f,
        SpriteEffects.None,
        0.9f);
    
    string label = Helper.Translation.Get("open_saves_folder");
    Vector2 textSize = Game1.smallFont.MeasureString(label);
    float textX = openFolderButton.bounds.X + 65;
    float textY = openFolderButton.bounds.Y + openFolderButton.bounds.Height / 2 - textSize.Y / 2;
    
    b.DrawString(Game1.smallFont, label,
        new Vector2(textX, textY),
        isButtonHovered ? Color.DarkBlue : Color.Black);
}
        
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu && openFolderButton != null)
            {
                if (e.Button == SButton.MouseLeft)
                {
                    int x = (int)e.Cursor.ScreenPixels.X;
                    int y = (int)e.Cursor.ScreenPixels.Y;
                    
                    if (openFolderButton.containsPoint(x, y))
                    {
                        Monitor.Log("Button clicked!", LogLevel.Info);
                        Game1.playSound("dwop");
                        OpenSavesFolder();
                        Helper.Input.Suppress(e.Button);
                    }
                }
            }
        }
        
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu && !buttonInitialized)
            {
                Monitor.Log("UpdateTicked: TitleMenu detected but button not initialized, initializing now...", LogLevel.Info);
                InitializeOpenFolderButton();
                buttonInitialized = true;
            }
            
            // ИЗМЕНЕНО: добавлена проверка Game1.currentLocation == null
            if (hasNewSaves && Game1.activeClickableMenu is TitleMenu && Game1.currentLocation == null)
            {
                Monitor.Log("Showing restart prompt in title menu", LogLevel.Info);
                hasNewSaves = false;
                Game1.activeClickableMenu = new RestartPromptMenu(this);
            }
        }
        
        public void RestartGame()
        {
            try
            {
                string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string gamePath = Path.GetFullPath(Path.Combine(modPath, "..", ".."));
                string exeName;
                
                if (OperatingSystem.IsWindows())
                    exeName = "StardewModdingAPI.exe";
                else if (OperatingSystem.IsMacOS())
                    exeName = "StardewModdingAPI";
                else
                    exeName = "StardewModdingAPI";
                
                string fullPath = Path.Combine(gamePath, exeName);
                
                if (File.Exists(fullPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullPath,
                        WorkingDirectory = gamePath,
                        UseShellExecute = true
                    });
                    
                    Environment.Exit(0);
                }
                else
                {
                    Monitor.Log($"Game executable not found at: {fullPath}", LogLevel.Error);
                    ShowManualRestartMessage();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to restart game: {ex.Message}", LogLevel.Error);
                ShowManualRestartMessage();
            }
        }
        
        private void ShowManualRestartMessage()
        {
            string message = Helper.Translation.Get("manual_restart");
            Game1.activeClickableMenu = new DialogueBox(message);
        }
        
        public void OpenSavesFolder()
        {
            try
            {
                string savesPath = Constants.SavesPath;
                
                if (Directory.Exists(savesPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = savesPath,
                        UseShellExecute = true
                    });
                    
                    Monitor.Log($"Opened saves folder: {savesPath}", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to open saves folder: {ex.Message}", LogLevel.Error);
            }
        }
    }
}