# DopplerHunter

![Descripción de la imagen](.github/assets/preview.png)

## Description

**Dopple Hunter** is a fast, lightweight desktop application designed to scan, detect, and manage duplicate files across your system. It helps you reclaim disk space and keep your storage organized by accurately grouping identical files using content-based hash analysis.

### 🚀 Key Features

 - Smart Hash Grouping: Automatically categorizes identical files into distinct visual groups for effortless review before taking action.
 - Flexible Directory Scanning: Add multiple target paths and include or exclude subdirectories while preventing duplicate processing of the same files.
 - Batch Selection: Quickly select duplicate copies across groups with a single click while keeping the original file safe.
 - Visual Inspection & Filters: Filter results by file extensions, view parent folders with clean UI indicators, and open file locations directly.
 - Resource-Efficient: Built to handle large volumes of files with minimal CPU and memory overhead.

### 🛠️ Built With

    Language: C#
    Framework: .NET / WPF (Windows Presentation Foundation)
    Architecture: MVVM Pattern (Model-View-ViewModel) 

### 🗃️ Installation

 1. Download the installation package from the [Releases](../../releases) section.
 2. Follow the standard installation wizard (Next, Next, Finish).

## 🚀 How to Use

1. **Select Folders:** Navigate the directory tree, highlight a folder, and press **Enter** to add it to the scan list.
2. **Configure Subdirectories:** Toggle whether to include or exclude subdirectories for each selected folder.
3. **Start Search:** Click **SearchDuplicates** to initiate the file analysis.
4. **Select Duplicates:** Review the grouped identical files and check the boxes for the items you wish to remove.
5. **Execute Deletion:** Click **Delete All Selected Duplicates**. 
   > **Note:** This action updates the *Action* column status to confirm whether each file was successfully deleted without removing them from the current view.
6. **Verify Clean-Up:** Click **SearchDuplicates** again to confirm all unwanted duplicate files have been cleared.