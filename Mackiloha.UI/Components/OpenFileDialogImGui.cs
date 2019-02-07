using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using ImGuiNET;

namespace Mackiloha.UI.Components
{
    public class OpenFileDialogImGui
    {
        public string CurrentDirectory { get; set; } = Directory.GetCurrentDirectory();
        public string StartDirectory { get; set; } = Directory.GetCurrentDirectory();

        public string CurrentPath { get; set; }
        public string SelectedPath { get; set; }
        public int LastClickedDirectoryIndex { get; set; } = -1;

        private static (string FullPath, string Name)[] GetParents(string path)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);

            return Enumerable.Range(1, parts.Length)
                .Select(x => (Path.Combine(parts.Take(x).ToArray()), parts[x - 1]))
                .ToArray();
        }

        private static (string FullPath, string Name, bool IsDirectory, int Index)[] GetImmediateChildren(string path)
        {
            var dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            int index = 0;
            
            return dirs.
                Select(x => (x, $"{Path.GetFileName(x)}{Path.DirectorySeparatorChar}", true, index++))
                .Union(files.Select(y => (y, Path.GetFileName(y), false, index++)))
                .ToArray();
        }

        public bool Render()
        {
            var parents = GetParents(CurrentDirectory);
            var children = GetImmediateChildren(CurrentDirectory);
            var childrenNames = children.Select(x => x.Name).ToArray();

            if (ImGui.IsMouseDoubleClicked(0))
            {
                if (LastClickedDirectoryIndex != -1)
                    CurrentDirectory = children[LastClickedDirectoryIndex].FullPath;
                else
                    LastClickedDirectoryIndex = -1;
            }


            ImGui.Text("Current:");
            
            foreach (var p in parents)
            {
                ImGui.SameLine();
                
                if (ImGui.Button(p.Name))
                {
                    CurrentDirectory = p.FullPath;
                    CurrentPath = "";
                    LastClickedDirectoryIndex = -1;
                }
            }

            /*
            ImGui.SameLine();
            if (ImGui.Button("", new System.Numerics.Vector2(-1, 20)))
            {
                //CurrentDirectory = p.FullPath;
            }*/

            ImGui.NewLine();
            
            // Finds index of selected file
            int idx = Enumerable.Range(0, children.Length)
                .Select(x => new { Child = children[x], Index = x })
                .Where(y => !y.Child.IsDirectory && y.Child.Name.Equals(CurrentPath, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault()?.Index ?? -1;
            
            ImGui.PushItemWidth(-1);
            if (ImGui.ListBox("##Files", ref idx, childrenNames, childrenNames.Length))
            {
                if (children[idx].IsDirectory)
                {
                    LastClickedDirectoryIndex = idx;
                }
                else
                {
                    CurrentPath = childrenNames[idx];
                    LastClickedDirectoryIndex = -1;
                }
            }
            
            var current = CurrentPath ?? "";
            if (ImGui.InputText("Current", ref current, 0xFF))
            {
                CurrentPath = current ?? "";
                LastClickedDirectoryIndex = -1;
            }

            //ImGui.SameLine();
            if (ImGui.Button("Open"))
            {
                ImGui.CloseCurrentPopup();
                LastClickedDirectoryIndex = -1;
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
                LastClickedDirectoryIndex = -1;
            }

            return true;
        }
    }
}
