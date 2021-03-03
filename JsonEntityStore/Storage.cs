using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace JsonEntityStore
{
    /// <summary>
    /// All objects serialized to this simple data store
    /// must have an int Id property that has a public
    /// getter and setter. Make your data entities
    /// implement this interface.
    /// </summary>

    public interface IID { int Id { get; set; } }

    /// <summary>
    /// A container class for the IComparer(T) needed
    /// to sort the loaded list of entities, and for
    /// doing efficient binary searches of the loaded list.
    /// </summary>
    /// <typeparam name="T">The type of entity being
    /// sorted or sought</typeparam>

    internal class JsonEntityComparer<T> : IComparer<T> where T : IID
    {
        public int Compare(T x, T y) => x.Id - y.Id;
    }

    /// <summary>
    /// Storage manager for the class T. Note that the
    /// class must implement the IID interface, and must
    /// contain simple data type properties compatible
    /// with NewtonSoft.Json serialization.
    /// </summary>
    /// <typeparam name="T">The property-rich data type
    /// to be serialized</typeparam>

    public class Storage<T> : IEnumerable<T> where T : IID, new()
    {
        List<T> cache;
        readonly string storageFolder;
        int nextFreeId;
        readonly IComparer<T> comparer;
        readonly bool useZip;

        /// <summary>
        /// Constructor. Must associate a storage with
        /// a folder into which/from which the objects
        /// are persisted as JSON files.
        /// </summary>
        /// <param name="folder">The folder where the
        /// objects are stored</param>
        /// <param name="zipped">True to store the data in a
        /// zip file, false for a pure JSON file</param>

        public Storage(string folder, bool zipped)
        {
            useZip = zipped;
            if (!Directory.Exists(folder))
                throw new ArgumentException
                    ($"Folder {folder} does not exist");
            storageFolder = folder;
            cache = All();
            if (cache.Any())
                nextFreeId = All().Max(ti => ti.Id) + 1;
            else
                nextFreeId = 1;
            comparer = new JsonEntityComparer<T>();
        }

        /// <summary>
        /// Get the entire table of objects
        /// </summary>
        /// <returns>The sorted list of all
        /// entities of type T in the collection
        /// </returns>

        public List<T> All()
        {
            if (cache == null)
                Read();
            return cache;
        }

        /// <summary>
        /// Reload the contents of the in memory entity
        /// List for type T. Overwrites any changes that
        /// have not been saved out to disk.
        /// </summary>

        public void Read()
        {
            nextFreeId = 1;
            if (useZip)
                ReadZip();
            else
            {
                if (File.Exists(ClassStoragePath))
                {
                    using var tr = new StreamReader(ClassStoragePath);
                    ReadJson(tr);
                }
                else
                    cache = new List<T>();
            }
        }

        private void ReadZip()
        {
            if (File.Exists(ClassStoragePath + ".zip"))
            {
                using ZipArchive zf = ZipFile
                    .OpenRead(ClassStoragePath + ".zip");
                foreach (ZipArchiveEntry ze in zf.Entries)
                {
                    using Stream s = ze.Open();
                    using var tr = new StreamReader(s);
                    ReadJson(tr);
                }
            }
            else
                cache = new List<T>();
        }

        private void ReadJson(TextReader reader)
        {
            using var jr = new JsonTextReader(reader);
            var js = new JsonSerializer();
            cache = js.Deserialize<List<T>>(jr);
            cache.Sort((t1, t2) => t1.Id - t2.Id);
            nextFreeId = cache[^1].Id + 1;
        }

        /// <summary>
        /// Search for the entity that has the supplied ID.
        /// This function uses a binary search, so is very
        /// efficient for looking up an object from the list.
        /// </summary>
        /// <param name="id">The ID for the entity we seek
        /// </param>
        /// <returns>The object that has the supplied unique
        /// ID, or null if the ID is not found in the
        /// collection</returns>

        public T Find(int id)
        {
            int index = cache.BinarySearch
                (new T { Id = id }, comparer);
            if (index >= 0)
                return cache[index];
            else return default;
        }

        /// <summary>
        /// Flush all changes out to the folder where
        /// these entities are persisted
        /// </summary>

        public void Save()
        {
            if (useZip)
                SaveZip();
            else
            {
                using var writer = new StreamWriter(ClassStoragePath, false);
                SaveJson(writer);
            }
        }

        private string ClassStoragePath =>
            Path.Combine(storageFolder, $"{typeof(T).FullName}.json");

        private void SaveZip()
        {
            using FileStream zipFile = new FileStream(ClassStoragePath + ".zip", FileMode.Create);
            using ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Update);
            ZipArchiveEntry zipEntry = archive.CreateEntry(ClassStoragePath);
            using var writer = new StreamWriter(zipEntry.Open());
            SaveJson(writer);
        }

        private void SaveJson(TextWriter writer)
        {
            var js = new JsonSerializer();
            using var jr = new JsonTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };
            js.Serialize(jr, cache);
        }

        /// <summary>
        /// To be used instead of the constructor for new
        /// instances of type T. This method creates a new
        /// entity, auto-allocates a unique Id to it, and
        /// adds it to the list of entities held by this
        /// storage object. The new T is returned to the
        /// caller.
        /// </summary>
        /// <returns>The newly created T</returns>

        public T New()
        {
            var newT = new T { Id = nextFreeId++ };
            cache.Add(newT);
            return newT;
        }

        /// <summary>
        /// Delete the object from the collection. If the
        /// object is not in the collection, we silently
        /// return without error.
        /// </summary>
        /// <param name="t">The object to be deleted.
        /// Note that it must be the actual object
        /// in the collection, not just an object
        /// with the same ID.</param>

        public void Delete(T t)
        {
            // Relies on the fact that default(T)
            // will never have been inserted, so a
            // null argument to Delete() will cause
            // nothing to be deleted.

            if (cache.Contains(t))
                cache.Remove(t);
        }

        /// <summary>
        /// Delete an object from the collection
        /// by its ID. 
        /// </summary>
        /// <param name="i"></param>

        public void Delete(int i)
        {
            T t = cache.FirstOrDefault(ti => ti.Id == i);
            Delete(t);
        }

        public IEnumerator<T> GetEnumerator() => All().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => All().GetEnumerator();
    }
}
