using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StealthGame.Persistence;
using StealthGame.UWP.Persistence;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(WindowsFilePersistence))]
namespace StealthGame.UWP.Persistence
{
    /// <summary>
    /// Storage for windows.
    /// </summary>
    public class WindowsFilePersistence : IStealthGameDataAccess
    {
        /// <summary>
        /// Loads the game.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>The StealthGameTable read from the file.</returns>
        public async Task<StealthGameTable> LoadAsync(string path)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(path);

                string text = await FileIO.ReadTextAsync(file);

                string[] values = text.Split(' ');

                int tableSize = int.Parse(values[0]);

                StealthGameTable table = new StealthGameTable(tableSize)
                {
                    Guards = new List<Tuple<int, int, int>>()
                };

                int val = 1;
                for (int i = 0; i < tableSize; ++i)
                {
                    for (int j = 0; j < tableSize; ++j)
                    {
                        table.SetValue(i, j, values[val]);

                        // Add a random starting direction to the guards
                        Random r = new Random();
                        int d = r.Next(0, 4);

                        if (values[val] == "G")
                        {
                            table.Guards.Add(new Tuple<int, int, int>(i, j, d));
                        }

                        ++val;
                    }
                }

                return table;
            }
            catch
            {
                throw new StealthGameDataException("Loading failed.");
            }
        }

        /// <summary>
        /// Saves the game.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="table">The StealthGameTable to write to the file.</param>
        public async Task SaveAsync(string path, StealthGameTable table)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);

                string text = table.TableSize.ToString();
                text += " ";

                for (int i = 0; i < table.TableSize; i++)
                {
                    for (int j = 0; j < table.TableSize; j++)
                    {
                        text += table.GetValue(i, j) + " ";
                    }
                }

                await FileIO.WriteTextAsync(file, text);
            }
            catch
            {
                throw new StealthGameDataException("Saving failed.");
            }
        }
    }
}