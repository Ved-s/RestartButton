using BepInEx;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using Music;
using System;
using UnityEngine;

namespace RestartButton
{
    [BepInPlugin("ved_s.restartbutton", "Restart Button", "1.0")]
    public class RestartButton : BaseUnityPlugin
    {
        static SimpleButton? Button;

        public void OnEnable()
        {
            On.Menu.PauseMenu.SpawnExitContinueButtons += PauseMenu_SpawnExitContinueButtons;
            On.Menu.PauseMenu.SpawnConfirmButtons += PauseMenu_SpawnConfirmButtons;
            On.Menu.PauseMenu.Singal += PauseMenu_Signal;
        }

        private void PauseMenu_Signal(On.Menu.PauseMenu.orig_Singal orig, PauseMenu self, MenuObject sender, string message)
        {
            if (message == "RESTART2" && self.game.session is StoryGameSession story)
            {
                RestartGame(self.manager, story.saveStateNumber);
            }

            orig(self, sender, message);
        }

        private void PauseMenu_SpawnExitContinueButtons(On.Menu.PauseMenu.orig_SpawnExitContinueButtons orig, PauseMenu self)
        {
            orig(self);

            if (self.game.session is StoryGameSession)
            {
                Button = new(self, self.pages[0], self.Translate("RESTART"), "RESTART2", new Vector2(self.ContinueAndExitButtonsXPos - 460.2f - self.manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(self.manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
                self.pages[0].subObjects.Add(Button);
            }
        }

        private void PauseMenu_SpawnConfirmButtons(On.Menu.PauseMenu.orig_SpawnConfirmButtons orig, PauseMenu self)
        {
            orig(self);
            if (Button is not null)
            {
                self.pages[0].subObjects.Remove(Button);
                Button?.RemoveSprites();
                Button = null;
            }
        }

        // Mostly a copy of SlugcatSelectMenu.StartGame
        public static void RestartGame(ProcessManager manager, SlugcatStats.Name name)
        {
            manager.arenaSitting = null;
            manager.rainWorld.progression.currentSaveState = null;
            manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = name;
            if (ModManager.CoopAvailable)
            {
                for (int j = 1; j < manager.rainWorld.options.JollyPlayerCount; j++)
                {
                    manager.rainWorld.RequestPlayerSignIn(j, null);
                }
                for (int k = manager.rainWorld.options.JollyPlayerCount; k < 4; k++)
                {
                    manager.rainWorld.DeactivatePlayer(k);
                }
            }

            manager.rainWorld.progression.WipeSaveState(name);
            manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
            if ((name != SlugcatStats.Name.White && name != SlugcatStats.Name.Yellow && (!ModManager.MSC || name != MoreSlugcatsEnums.SlugcatStatsName.Saint)) || Input.GetKey("s"))
            {
                manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
            }
            else
            {
                if (name == SlugcatStats.Name.Yellow)
                {
                    manager.nextSlideshow = SlideShow.SlideShowID.YellowIntro;
                }
                else if (ModManager.MSC && name == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    manager.nextSlideshow = MoreSlugcatsEnums.SlideShowID.SaintIntro;
                }
                else
                {
                    manager.nextSlideshow = SlideShow.SlideShowID.WhiteIntro;
                }
                manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
            }

            if (manager.menuMic != null)
            {
                manager.menuMic.PlaySound(SoundID.MENU_Start_New_Game);
            }
            else if (manager.currentMainLoop is RainWorldGame game)
            {
                game.cameras[0].virtualMicrophone.PlaySound(SoundID.MENU_Start_New_Game, 0f, 1f, 1f, 1);
            }

            if (manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song is IntroRollMusic)
            {
                manager.musicPlayer.song.FadeOut(20f);
            }
        }
    }
}
