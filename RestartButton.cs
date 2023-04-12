using BepInEx;
using Menu;
using Menu.Remix;
using Music;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RestartButton
{
    [BepInPlugin("ved_s.restartbutton", "Restart Button", "1.0")]
    public class RestartButton : BaseUnityPlugin
    {
        static OpMutedHoldButton? Button;
        static MenuTabWrapper? TabWrapper;
        static UIelementWrapper? ButtonWrapper;
        static PauseMenu? ButtonMenu;

        public void OnEnable()
        {
            On.Menu.PauseMenu.SpawnExitContinueButtons += PauseMenu_SpawnExitContinueButtons;
            On.Menu.PauseMenu.SpawnConfirmButtons += PauseMenu_SpawnConfirmButtons;
        }

        private void PauseMenu_SpawnExitContinueButtons(On.Menu.PauseMenu.orig_SpawnExitContinueButtons orig, PauseMenu self)
        {
            orig(self);

            if (self.game.session is StoryGameSession)
            {
                TabWrapper = new(self, self.pages[0]);
                self.pages[0].subObjects.Add(TabWrapper);

                Button = new(new Vector2(self.ContinueAndExitButtonsXPos - 460.2f - self.manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(self.manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f), self.Translate("RESTART"));
                Button.description = " ";
                Button.OnPressDone += Button_OnPressDone;
                ButtonWrapper = new(TabWrapper, Button);
                ButtonMenu = self;
            }
        }

        private void Button_OnPressDone(Menu.Remix.MixedUI.UIfocusable trigger)
        {
            if (ButtonMenu?.game.session is StoryGameSession story)
            {
                RestartGame(ButtonMenu.manager, story.saveStateNumber);
            }
        }

        private void PauseMenu_SpawnConfirmButtons(On.Menu.PauseMenu.orig_SpawnConfirmButtons orig, PauseMenu self)
        {
            orig(self);
            if (TabWrapper is not null)
            {
                self.pages[0].subObjects.Remove(TabWrapper);
                TabWrapper?.RemoveSprites();
                TabWrapper = null;
            }
            if (ButtonWrapper is not null)
            {
                ButtonWrapper?.RemoveSprites();
                ButtonWrapper = null;
            }
            Button = null;
            ButtonMenu = null;
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
            if ((name != SlugcatStats.Name.White && name != SlugcatStats.Name.Yellow && (!ModManager.MSC || name != MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)) || Input.GetKey("s"))
            {
                manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
            }
            else
            {
                if (name == SlugcatStats.Name.Yellow)
                {
                    manager.nextSlideshow = SlideShow.SlideShowID.YellowIntro;
                }
                else if (ModManager.MSC && name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    manager.nextSlideshow = MoreSlugcats.MoreSlugcatsEnums.SlideShowID.SaintIntro;
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
