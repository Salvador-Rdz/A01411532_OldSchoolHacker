﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class Hacker : MonoBehaviour {
    #region Class Attributes
    //Attributes
    //Tool strings to contain commonly used messages
    const string menuHint = "You can type menu anytime to disconnect";
    const string progressDefault = "Progress: ";
    const string attemptDefault = "REMAINING ATTEMPTS: ";
    //Contains the current status warning text. 
    string statusText = "!FIREWALL DETECTED!";
    //Counter of level progress, changes depending on the difficulty.
    int progress = 0;
    //Self-explanatory variables.
    string input;
    int level;
    string password;
    bool changePassword;
    public double money = 0;
    int attempts;
    //Contains the passwords for each difficulty level
    string[] passwordsLevel1;
    string[] passwordsLevel2;
    string[] passwordsLevel3;
    //.txt files that cointain the passwords that are then loaded into the game. Linked via the inspector.
    public TextAsset[] levels;
    //enumerators for the possible states of the game, useful for determining how to process input
    enum GameState { MainMenu,Gameplay,Win, Loss};
    GameState currentState = GameState.MainMenu;
    //References to GameObjects used to manage UI and Sound elements. 
    private AudioManager audioMgr;
    public Text warningText;
    public Text attemptsText;
    public Text progressText;
    #endregion
    // Use this for initialization
    void Start () {
        ShowMainMenu();
        LoadLevels();
    }
    private void Awake()
    {
        //Initiate the audiomanager, finds the "AdminManager" object in the GameObject Hierarchy to utilize its code and resources. Mainly the EG and game sounds
        audioMgr = FindObjectOfType<AudioManager>();
        //Hides a few labels and objects.
        attemptsText.enabled = false;
    }
    private void ShowMainMenu()
    {
        audioMgr.Stop("alarm");
        //Flavor text
        Terminal.ClearScreen();
        Terminal.WriteLine("Welcome back Operator");
        Terminal.WriteLine("these are the missions for today");
        Terminal.WriteLine("");
        Terminal.WriteLine("1. Accessing a hospital database");
        Terminal.WriteLine("2. Hijacking a IT security center");
        Terminal.WriteLine("3. Toppling down Taito's SecNet System");
        Terminal.WriteLine("");
        Terminal.WriteLine("Choose an option.");
        //Reutilizing the Text label to display money on the main menu
        progressText.text = "Bank : " + money + " btc";
        //We reset all of our tracking variables
        level = 0;
        currentState = GameState.MainMenu;
        changePassword = true;
        progress = 0;
        attempts = 1;
        //Reset all of the UI elements
        warningText.text = "ALL SYSTEMS STABLE";
        attemptsText.enabled = false;
    }

    // Update is called once per frame
    void Update () {
		
	}
    void OnUserInput(string input)
    {
        // if user inputs the "menu" keyword, then we'll showMainMenu()
        if(input == "menu")
        {
            ShowMainMenu();
        }
        //If the user types quit, close, or exit, then we exit the game. If the game is played on a browser, then we ask the user to
        //close the user
        else if (input == "quit" || input == "close" || input == "exit")
        {
            Terminal.WriteLine("Please, close the browser tab");
            Application.Quit();
        }
        //If the user inputs anything that is not menu, quit, close or exit
        //then we are going to handle that input depending on the game state.
        //if the game state is still mainmenu, then we call the RunMainMenu()
        else if(currentState == GameState.MainMenu)
        {
            RunMainMenu(input);
        }
        //But if the current game state is the password
        else if(currentState == GameState.Gameplay)
        {
            CheckPassword(input);
        }
    }

    private void CheckPassword(string input)
    {
        if (input == "menu")
        {
            
            ShowMainMenu();
        }
        //Easter Egg, plays the game over speech from MGS when inputting "snake"
        if(input == "snake")
        {
            audioMgr.Play("Snake");
        }
        if (input == password)
        {
            //We set up a progress bar and progress values, these change with the difficulty level.
            progress++;
            progressText.text = progressDefault + progress + "/" + (level * 2);
            //Depending on the difficulty level, the words that must be guessed increase.
            if (level == 1 && progress == 2)
            {
                DisplayWinScreen();
            }
            else if (level == 2 && progress == 4)
            {
                DisplayWinScreen();
                audioMgr.Stop("alarm");
            }
            else if (level == 3 && progress == 6)
            {
                DisplayWinScreen();
                audioMgr.Stop("alarm");
            }
            else
            {
                changePassword = true;
                AskForPassword();
            }
        }
        else
        {
            //If the player doesn't get the password right, the attempts counter goes down. 
            attempts--;
            //This upates the text only if the tag is enabled, and it's only enabled if on levels 2 or higher.
            if (attemptsText.enabled) attemptsText.text = attemptDefault + attempts;
            //If there are no remaining attempts and the player isn't on level 1
            if (attempts == 0 && level!=1)
            {
                //We change to the Loss status.
                DisplayLossScreen();
            }
            else
            {
                AskForPassword();
            }
        }
    }
    //Displays flavor text and removes currency from the player depending on the difficulty
    private void DisplayLossScreen()
    {
        audioMgr.Stop("alarm");
        //Setting up the game state and console display
        currentState = GameState.Loss;
        Terminal.ClearScreen();
        Terminal.WriteLine(menuHint);
        //Setting up the header's blinking text
        warningText.text = "FAILSAFE ACTIVATED";
        StopAllCoroutines();
        StartCoroutine(BlinkText());
        //Flavor text
        Terminal.WriteLine("Damn it operator!");
        Terminal.WriteLine("That's going to cost you.");
        Terminal.WriteLine("Lost "+(level * 2)+" btc");
        Terminal.WriteLine("Be more careful next time!");
        //Money is deducted from the player's account
        money -= level * 2;
    }
    //Displays the win screen flavor text
    private void DisplayWinScreen()
    {
        currentState = GameState.Win;
        Terminal.ClearScreen();
        Terminal.WriteLine(menuHint);
        ShowLevelReward();
    }
    //Prints the reward flavor text and adds money to the player's bank depending on the level they completed
    private void ShowLevelReward()
    {
        switch(level)
        {
            case 1:
                Terminal.WriteLine("Piece of cake!");
                Terminal.WriteLine("0.005 btc have been transferred to your account");
                money += .005;
                level = 0;
                break;
            case 2:
                Terminal.WriteLine("Job well done!");
                Terminal.WriteLine("0.3 btc have been transferred to your account");
                money += 0.3;
                level = 0;
                break;
            case 3:
                Terminal.WriteLine("Good show operator!");
                Terminal.WriteLine("10 btc have been transferred to your account");
                money += 10;
                level = 0;
                break;
        }
    }

    private void RunMainMenu(string input)
    {
        //Check if the input is valid
        bool isValidInput = (input == "1") || (input == "2") || (input == "3");
        //if the user inputs a valid level, we convert that input to an int value and then we call the AskForPassword() method.
        if (isValidInput)
        {
            progressText.enabled = true;
            level = int.Parse(input);
            progressText.text = progressDefault + progress +"/" +(level*2);
            //Player will have limited attempts when trying harder levels, they will fail if they can't guess all of the words within the attempt limits
            if (input != "1")
            {
                //Calculating the attempts
                attempts = 18 / level;
                //Setting up the text
                attemptsText.enabled = true;
                attemptsText.text = attemptDefault + attempts;
                //Setting up the 'Firewall' flavor text
                warningText.text = statusText;
                StopAllCoroutines();
                StartCoroutine(BlinkText());
                //Setting up the warning sounds
                audioMgr.Play("alarm");
            }
            AskForPassword();
        }
        //However, if the user did not enter a valid input, then we validate for our Easter Eggs
        else if(input =="1337")
        {
            Terminal.WriteLine("Pl3453 3nt3r 4 v4l1d l3v3l:");
        }
        else
        {
            Terminal.WriteLine("Enter a valid level");
        }
    }

    private void AskForPassword()
    {
        currentState = GameState.Gameplay;
        Terminal.ClearScreen();
        if(changePassword) SetRandomPassword();
        Terminal.WriteLine(menuHint);
        Terminal.WriteLine("Enter your password, Hint: " + password.Anagram());
    }

    private void SetRandomPassword()
    {
        changePassword = false;
        switch(level)
        {
            case 1:
                password = passwordsLevel1[UnityEngine.Random.Range(0, passwordsLevel1.Length)];
                break;
            case 2:
                password = passwordsLevel2[UnityEngine.Random.Range(0, passwordsLevel2.Length)];
                break;
            case 3:
                password = passwordsLevel3[UnityEngine.Random.Range(0, passwordsLevel3.Length)];
                break;
            default:
                changePassword = true;
                Debug.Log("Invalid level, contact the developer for some ass-kicking");
                break;
        }
    }
    //Small function to empty the text file assets into the on-runtime passwords array.
    void LoadLevels()
    {
        passwordsLevel1 = levels[0].text.Split();
        passwordsLevel2 = levels[1].text.Split();
        passwordsLevel3 = levels[2].text.Split();
    }
    //corroutine function to blink the text
    public IEnumerator BlinkText()
    {
        warningText.enabled = true;
        while (level == 2 || level == 3)
        {
            //Get the current text to blink
            string currentText = warningText.text;
            //set the Text's text to blank
            warningText.text = "";
            //display blank text for 0.5 seconds
            yield return new WaitForSeconds(.5f);
            //display the current warning text for the next 0.5 seconds
            warningText.text = currentText;
            yield return new WaitForSeconds(.5f);
        }
    }
}
