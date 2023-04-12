using BepInEx;
using DevInterface;
using Menu;
using Menu.Remix;
using Music;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RestartButton
{
    [BepInPlugin("ved_s.restartbutton", "Restart Button", "1.2")]
    public class RestartButton : BaseUnityPlugin
    {
        static OpMutedHoldButton? Button;
        static MenuTabWrapper? TabWrapper;
        static UIelementWrapper? ButtonWrapper;
        static ProcessManager? Manager;
        static SlugcatStats.Name? SlugcatName;

        public void OnEnable()
        {
            On.Menu.PauseMenu.SpawnExitContinueButtons += PauseMenu_SpawnExitContinueButtons;
            On.Menu.PauseMenu.SpawnConfirmButtons += PauseMenu_SpawnConfirmButtons;
            On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;
        }

        private void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, SleepAndDeathScreen self, KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            orig(self, package);

            if (self.IsDeathScreen)
            {
                AddButton(self, self.pages[0], self.manager, package.saveState.saveStateNumber, self.ContinueAndExitButtonsXPos);
            }
        }

        private static void PauseMenu_SpawnExitContinueButtons(On.Menu.PauseMenu.orig_SpawnExitContinueButtons orig, PauseMenu self)
        {
            orig(self);

            if (self.game.session is StoryGameSession story)
            {
                AddButton(self, self.pages[0], self.manager, story.saveStateNumber, self.ContinueAndExitButtonsXPos);
            }
        }
        private static void PauseMenu_SpawnConfirmButtons(On.Menu.PauseMenu.orig_SpawnConfirmButtons orig, PauseMenu self)
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
            Manager = null;
            SlugcatName = null;
        }

        private static void Button_OnPressDone(Menu.Remix.MixedUI.UIfocusable trigger)
        {
            if (Manager is not null && SlugcatName is not null)
            {
                RestartGame(Manager, SlugcatName);
            }
        }

        public static void AddButton(Menu.Menu menu, Menu.Page page, ProcessManager manager, SlugcatStats.Name name, float xpos)
        {
            TabWrapper = new(menu, page);
            page.subObjects.Add(TabWrapper);

            Button = new(new Vector2(xpos - 460.2f - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f), menu.Translate("RESTART"));
            Button.description = " ";
            Button.OnPressDone += Button_OnPressDone;
            ButtonWrapper = new(TabWrapper, Button);

            Manager = manager;
            SlugcatName = name;
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
