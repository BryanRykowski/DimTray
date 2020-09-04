using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DimTrayFramework
{

    public class Profile
    {
        public string FilePath { get; private set;}
        public List<short> Values { get; set;}

        public Profile(string path)
        {
            FilePath = path;
            Values = new List<short>();
        }

        public Profile(string path, List<short> values)
        {
            FilePath = path;
            Values = values;
        }

        public bool Serialize()
        {
                using (BinaryWriter bw = new BinaryWriter(File.Open(FilePath, FileMode.Create)))
                {
                    try
                    {
                        bw.Write(Values.Count);
                
                        foreach (short val in Values)
                        {
                            bw.Write(val);
                        }
                        return true;
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show("Failed to create profile at \"" + FilePath + "\" with exception:\n" + e.Message);

                        return false;
                    }
                }
        }

        public bool DeSerialize()
        {
            if (File.Exists(FilePath))
            {
                Values.Clear();

                try
                {
                using(BinaryReader br = new BinaryReader(File.Open(FilePath, FileMode.Open)))
                {
                    int Length = br.ReadByte();

                    for (int i = 0; i < Length; ++i)
                    {
                        Values.Append(br.ReadInt16());
                    }
                        return true;
                }

                }
                catch(Exception e)
                {
                    MessageBox.Show("Failed to retrieve profile at \"" + FilePath + "\" with exception:\n" + e.Message);

                    return false;
                }
            }
            else 
            {
                return false;
            }
        }
    }

    public class ProfileList
    {
        string DirectoryPath;
        public List<Profile> Profiles { get; private set;}

        public ProfileList(string path)
        {
            DirectoryPath = path;
            Profiles = new List<Profile>();
        }

        public void Clear()
        {
            Profiles.Clear();
        }

        public bool Refresh()
        {
            if (Directory.Exists(DirectoryPath))
            {
                try 
                {
                    IEnumerable<string> Files;

                    Files = Directory.EnumerateFiles(DirectoryPath, "*.DTprofile");

                    Profiles.Clear();

                    foreach (string file in Files)
                    {
                        Profile profile = new Profile(file);
                        profile.DeSerialize();

                        Profiles.Append(profile);
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show("Failed to retrieve profiles from\"" + DirectoryPath + "\" with exception:\n" + e.Message);

                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeleteProfile(int index)
        {
            try
            {
                File.Delete(Profiles[index].FilePath);

                this.Refresh();

                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show("Failed to delete file \"" + Profiles[index].FilePath + "\" with exception:\n" + e.Message);

                return false;
            }

        }

        public bool AddProfile(string name, List<short> values)
        {
            Profile profile = new Profile(DirectoryPath + name, values);

            bool result = profile.Serialize();

            this.Refresh();

            return result;
        }
    }
}
