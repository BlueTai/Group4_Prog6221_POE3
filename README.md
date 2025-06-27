# SecBot: Cybersecurity Awareness Chatbot

Author: Taahir Basson St10282092

Date 2025/06/27


This is a GUI-based Cybersecurity Awareness Chatbot built using C# and Windows Presentation Foundation (WPF). It is designed to be an educational tool that helps users learn about cybersecurity best practices in an interactive way.

The application includes the following key features:

1.  **Interactive Chatbot:** A conversational agent that provides information on topics like strong passwords, phishing, and online privacy.
2.  **Task Assistant with Reminders:** A dedicated GUI panel where users can manage and set reminders for cybersecurity-related tasks (e.g., "Enable two-factor authentication").
3.  **Natural Language Processing (NLP) Simulation:** The chatbot can understand user commands and questions even if they are phrased differently, making the interaction more natural.
4.  **Cybersecurity Mini-Game (Quiz):** A short quiz to test the user's knowledge on key cybersecurity concepts.
5.  **Activity Log:** A log that records all significant user and chatbot actions for review.

Installation and Setup

1.  Prerequisites
    * Visual Studio 2022 or later
    * .NET 8.0 SDK (or the target framework of the project)
    * The `greeting.wav` file located at the specified path (`C:\Users\bluet\source\repos\prog6221-poe-BlueTai\SecBot\greeting.wav`).

2.   Repository and youtube vidoee
   https://youtu.be/qvass2H45Co
https://github.com/BlueTai/Group4_Prog6221_POE3.git

4.  Open in Visual Studio
    Open the `.sln` file in Visual Studio.

5.  Build and Run
    Press `F5` to build and run the application.

Usage

* **Chat:** Type your questions in the textbox at the bottom left and press "Send" or `Enter`.
* **Task Assistant:** Use the controls in the right-hand "Task Assistant" tab to add and manage your tasks.
* **Quiz:** Type "start quiz" to begin the cybersecurity quiz.
* **Activity Log:** Switch to the "Activity Log" tab to see a record of all significant actions.

Documentation and Code Structure

* **`MainWindow.xaml`:** Defines the user interface layout using XAML.
* **`MainWindow.xaml.cs`:** The code-behind file containing all the C# logic for the chatbot's functionality, task management, quiz, and logging.
* **`SecBot` folder:** Contains the project files and assets.

Commit History

This project's development followed an agile approach, with commits documenting the implementation of each feature:

* `feat: Add GUI-based Task Assistant with reminders`
* `feat: Implement flexible NLP simulation with keyword detection`
* `feat: Create a Cybersecurity Mini-Game (Quiz)`
* `feat: Add comprehensive Activity Log feature`
* `feat: Cohesive integration and polish`

