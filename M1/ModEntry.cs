﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using StardewValley.TerrainFeatures;


namespace M1
{
	public class ModEntry : Mod {
		public static IModHelper helper;
		public static ModEntry instance;

		Shell shell;

		public override void Entry(IModHelper helper) {
			instance = this;
			ModEntry.helper = helper;
			helper.Events.Display.MenuChanged += this.OnMenuChanged;
			helper.Events.GameLoop.UpdateTicking += UpdateTicking;
			helper.Events.Input.ButtonPressed += this.OnButtonPressed;
		}

		private void UpdateTicking(object sender, UpdateTickingEventArgs e) {
			if (bot != null) bot.Update();
		}

		public void print(string s) {
            this.Monitor.Log(s, LogLevel.Debug);
		}

		Bot bot;
		public void OnButtonPressed(object sender, ButtonPressedEventArgs e) {
			if (e.Button == SButton.PageUp) {
				// Create a bot.
				bot = new Bot();
				Vector2 pos = Game1.player.position;
				pos.X -= 64;
				Game1.currentLocation.dropObject(bot, pos, Game1.viewport, true, (Farmer)null);
				bot.NotePosition();

			}
			if (e.Button == SButton.PageDown && bot != null) {
				// This works!  There's no tool animation, of course, but it does/
				// have the effect of using that tool on the environment.  Neat!
				bot.UseTool();
				bot.Move(-1, 0);
			}
			if (e.Button == SButton.Left && bot != null) bot.Move(-1, 0);
			if (e.Button == SButton.Right && bot != null) bot.Move(1, 0);
			if (e.Button == SButton.Up && bot != null) bot.Move(0, -1);
			if (e.Button == SButton.Down && bot != null) bot.Move(0, 1);
		}

		public void OnMenuChanged(object sender, MenuChangedEventArgs e) {
			var dlog = e.NewMenu as DialogueBox;
			if (dlog == null || !dlog.isQuestion || dlog.responses[0].responseKey != "Weather") return;

			// insert our new response
			Response r = new Response("M1", "MiniScript M-1 Home Computer");
			dlog.responses.Insert(dlog.responses.Count-1, r);
			// adjust the dialog height
			var h = SpriteText.getHeightOfString(r.responseText, dlog.width) + 16;
			dlog.heightForQuestions += h; dlog.height += h;
			// intercept the handler (but call the original one for other responses)
			var prevHandler = Game1.currentLocation.afterQuestion;
			Game1.currentLocation.afterQuestion = (who, whichAnswer) => {
				print($"{who} selected channel {whichAnswer}");
				if (whichAnswer == "M1") PresentComputer();
				else prevHandler(who, whichAnswer);
			};
		}

		private void PresentComputer() {
			if (shell == null) shell = new Shell();
			shell.Present();

			var farm = (Farm)Game1.getLocationFromName("Farm");

			var layer = farm.map.Layers[0];
			shell.PrintLine($"Farm size: {layer.LayerWidth} x {layer.LayerHeight}");
			shell.PrintLine($"Farm animals: {farm.getAllFarmAnimals().Count}");
			shell.PrintLine($"Buildings: {farm.buildings.Count}");

			int featureCount = 0;
			int trees=0, bushes=0, grasses=0, hoeDirts=0, paths=0;
			var hoeLocs = new List<string>();
			foreach (KeyValuePair<Vector2, TerrainFeature> kvp in farm.terrainFeatures.Pairs) {
				if (kvp.Value is Tree) trees++;
				else if (kvp.Value is Bush) bushes++;
				else if (kvp.Value is Grass) grasses++;
				else if (kvp.Value is HoeDirt) {
					hoeDirts++;
					hoeLocs.Add(kvp.Key.ToString());	// locations are integers, X right and Y down from top-left
				}
				else if (kvp.Value is Flooring) paths++;
				featureCount++;
			}
			shell.PrintLine($"Trees: {trees}");
			shell.PrintLine($"Bushes: {bushes}");
			shell.PrintLine($"Grass: {grasses}");
			shell.PrintLine($"Tilled Ground: {hoeDirts}");// at: {string.Join(',', hoeLocs)}");
			shell.PrintLine($"Paved: {paths}");
			shell.PrintLine($"Total features: {featureCount}");

		}
	}
}
