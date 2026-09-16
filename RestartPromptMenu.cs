using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

using Color = Microsoft.Xna.Framework.Color;

namespace SaveFolderShortcut
{
    public class RestartPromptMenu : IClickableMenu
    {
        private ModEntry mod;
        private ClickableTextureComponent yesButton;
        private ClickableTextureComponent noButton;
        
        public RestartPromptMenu(ModEntry mod) : base(
            Game1.viewport.Width / 2 - 400,
            Game1.viewport.Height / 2 - 200,
            800, 400, true)
        {
            this.mod = mod;
            
            
yesButton = new ClickableTextureComponent(
    "Yes",
    new Rectangle(xPositionOnScreen + 100, yPositionOnScreen + 300, 250, 80),
    null, null, Game1.mouseCursors,
    new Rectangle(365, 494, 12, 11), 4f)
{ myID = 101 };

noButton = new ClickableTextureComponent(
    "No",
    new Rectangle(xPositionOnScreen + 450, yPositionOnScreen + 300, 250, 80),
    null, null, Game1.mouseCursors,
    new Rectangle(365, 494, 12, 11), 4f)
{ myID = 102 };
        }
        
        public override void draw(SpriteBatch b)
{
    drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18),
        xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f);
    
    // Заголовок
    string title = mod.Helper.Translation.Get("restart_title");
    Vector2 titleSize = Game1.smallFont.MeasureString(title);
    b.DrawString(Game1.smallFont, title,
        new Vector2(xPositionOnScreen + width / 2 - titleSize.X / 2, yPositionOnScreen + 30),
        Color.Black);
    
    // Сообщение
    string message = mod.Helper.Translation.Get("restart_message");
    float messageY = yPositionOnScreen + 100;
    
    string[] lines = WrapText(message, width - 100, Game1.smallFont);
    foreach (string line in lines)
    {
        Vector2 lineSize = Game1.smallFont.MeasureString(line);
        b.DrawString(Game1.smallFont, line,
            new Vector2(xPositionOnScreen + width / 2 - lineSize.X / 2, messageY),
            Color.Black);
        messageY += lineSize.Y + 5;
    }
    
    // Рисуем кнопки с иконками и текстом на одной линии
    string yesText = mod.Helper.Translation.Get("restart_yes");
    string noText = mod.Helper.Translation.Get("restart_no");
    
    float iconScale = 4f;
    float iconWidth = 12 * iconScale;  // 48px
    float iconHeight = 11 * iconScale; // 44px
    float gap = 10;  // отступ между иконкой и текстом
    
    // Кнопка "Да"
    float yesCenterY = yesButton.bounds.Y + yesButton.bounds.Height / 2;
    float yesTextWidth = Game1.smallFont.MeasureString(yesText).X;
    float yesTotalWidth = iconWidth + gap + yesTextWidth;
    float yesStartX = yesButton.bounds.X + (yesButton.bounds.Width - yesTotalWidth) / 2;
    
    b.Draw(Game1.mouseCursors,
        new Vector2(yesStartX, yesCenterY - iconHeight / 2),
        new Rectangle(365, 494, 12, 11),
        Color.White, 0f, Vector2.Zero, iconScale, SpriteEffects.None, 0.9f);
    
    b.DrawString(Game1.smallFont, yesText,
        new Vector2(yesStartX + iconWidth + gap, yesCenterY - Game1.smallFont.MeasureString(yesText).Y / 2),
        Color.Black);
    
    // Кнопка "Нет"
    float noCenterY = noButton.bounds.Y + noButton.bounds.Height / 2;
    float noTextWidth = Game1.smallFont.MeasureString(noText).X;
    float noTotalWidth = iconWidth + gap + noTextWidth;
    float noStartX = noButton.bounds.X + (noButton.bounds.Width - noTotalWidth) / 2;
    
    b.Draw(Game1.mouseCursors,
        new Vector2(noStartX, noCenterY - iconHeight / 2),
        new Rectangle(365, 494, 12, 11),
        Color.White, 0f, Vector2.Zero, iconScale, SpriteEffects.None, 0.9f);
    
    b.DrawString(Game1.smallFont, noText,
        new Vector2(noStartX + iconWidth + gap, noCenterY - Game1.smallFont.MeasureString(noText).Y / 2),
        Color.Black);
    
    base.draw(b);
    
    // Курсор
    b.Draw(Game1.mouseCursors, 
        new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), 
        new Rectangle(0, 0, 16, 16), 
        Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
}
        
        private string[] WrapText(string text, float maxWidth, SpriteFont font)
        {
            var lines = new System.Collections.Generic.List<string>();
            var words = text.Split(' ');
            string currentLine = "";
            
            foreach (string word in words)
            {
                string testLine = currentLine == "" ? word : currentLine + " " + word;
                Vector2 testSize = font.MeasureString(testLine);
                
                if (testSize.X <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (currentLine != "")
                    {
                        lines.Add(currentLine);
                    }
                    currentLine = word;
                }
            }
            
            if (currentLine != "")
            {
                lines.Add(currentLine);
            }
            
            return lines.ToArray();
        }
        
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (yesButton.containsPoint(x, y))
            {
                mod.RestartGame();
            }
            else if (noButton.containsPoint(x, y))
            {
                exitThisMenu();
            }
        }
        
        public override void performHoverAction(int x, int y)
        {
            yesButton.tryHover(x, y);
            noButton.tryHover(x, y);
            base.performHoverAction(x, y);
        }
    }
}