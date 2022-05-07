using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace DimTray
{
    class ProfileData
    {
        public int version { get; set; }
        public List<short> brightnessVals { get; set; } = new List<short>();
}

    class Profile
    {
        public string path;
        public string name;
        public ProfileData data;
    }

    class ProfileManager
    {
        public List<Profile> profiles = new List<Profile>();

        public void GetProfiles()
        {
            profiles.Clear();

            string profilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dimtray\\Profiles";

            if (Directory.Exists(profilePath))
            {
                string[] files = Directory.GetFiles(profilePath, "*.json");

                foreach(var file in files)
                {
                    Profile profile = new Profile();
                    profile.name = Path.GetFileNameWithoutExtension(file);
                    profile.path = file;

                    string json = File.ReadAllText(file);

                    try
                    {
                        profile.data = JsonSerializer.Deserialize<ProfileData>(json);
                    }
                    catch 
                    {
                        continue;
                    }

                    profiles.Add(profile);
                }
            }
        }

        public bool SaveNewProfile(List<short> vals, String name)
        {
            string appDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dimtray";
            string profileDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dimtray\\Profiles";
            string filePath = profileDir + "\\" + name + ".json";

            if (!Directory.Exists(appDir))
            {
                try
                {
                    Directory.CreateDirectory(appDir);
                }
                catch
                {
                    MessageBox.Show("Unable to create AppData\\Local\\DimTray directory!", "DimTray - Error");
                    return false;
                }
            }

            if (!Directory.Exists(profileDir))
            {
                try
                {
                    Directory.CreateDirectory(profileDir);
                }
                catch
                {
                    MessageBox.Show("Unable to create AppData\\Local\\DimTray\\Profiles directory!", "DimTray - Error");
                    return false;
                }
            }

            bool overwrite = true;

            if (File.Exists(filePath))
            {
                var buttons = MessageBoxButtons.YesNo;
                var result = MessageBox.Show("Profile: " + name + " already exists. Replace this profile?", "DimTray - Error", buttons);

                if (result == DialogResult.No)
                {
                    overwrite = false;
                }
            }

            if (overwrite)
            {
                var data = new ProfileData();

                data.version = 0;

                foreach (var val in vals)
                {
                    data.brightnessVals.Add(val);
                }

                var jsonStr = JsonSerializer.Serialize(data);

                try
                {
                    File.WriteAllText(filePath, jsonStr);
                }
                catch
                {
                    MessageBox.Show("Failed to save profile!", "DimTray - Error");
                    return false;
                }
            }

            return true;
        }
    }
}
