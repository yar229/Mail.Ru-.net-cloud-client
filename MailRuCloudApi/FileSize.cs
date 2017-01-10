//-----------------------------------------------------------------------
// <created file="FileSize.cs">
//     Mail.ru cloud client created in 2016.
// </created>
// <author>Korolev Erast.</author>
//-----------------------------------------------------------------------

using System;

namespace MailRuCloudApi
{
    /// <summary>
    /// File size definition.
    /// </summary>
    public struct FileSize : IEquatable<FileSize>
    {
        public FileSize(long defaultValue) : this()
        {
            _defValue = defaultValue;
            SetNormalizedValue();
        }

        /// <summary>
        /// Private variable for default value.
        /// </summary>
        private readonly long _defValue;

        /// <summary>
        /// Gets default size in bytes.
        /// </summary>
        /// <value>File size.</value>
        public long DefaultValue => _defValue;

        /// <summary>
        /// Gets normalized  file size, auto detect storage unit.
        /// </summary>
        /// <value>File size.</value>
        public float NormalizedValue { get; private set; }

        /// <summary>
        /// Gets auto detected storage unit by normalized value.
        /// </summary>
        public StorageUnit NormalizedType { get; private set; }



        /// <summary>
        /// Normalized value detection and auto detection storage unit.
        /// </summary>
        private void SetNormalizedValue()
        {
            if (_defValue < 1024L)
            {
                NormalizedType = StorageUnit.Byte;
                NormalizedValue = _defValue;
            }
            else if (_defValue >= 1024L && _defValue < 1024L * 1024L)
            {
                NormalizedType = StorageUnit.Kb;
                NormalizedValue = _defValue / 1024f;
            }
            else if (_defValue >= 1024L * 1024L && _defValue < 1024L * 1024L * 1024L)
            {
                NormalizedType = StorageUnit.Mb;
                NormalizedValue = _defValue / 1024f / 1024f;
            }
            else if (_defValue >= 1024L * 1024L * 1024L && _defValue < 1024L * 1024L * 1024L * 1024L)
            {
                NormalizedType = StorageUnit.Gb;
                NormalizedValue = _defValue / 1024f / 1024f / 1024f;
            }
            else
            {
                NormalizedType = StorageUnit.Tb;
                NormalizedValue = _defValue / 1024f / 1024f / 1024f / 1024f;
            }
        }

        #region == Equality ===================================================================================================================
        public static implicit operator FileSize(long defaultValue)
        {
            return new FileSize(defaultValue);
        }

        public static FileSize operator +(FileSize first, FileSize second)
        {
            return new FileSize(first.DefaultValue + second.DefaultValue);
        }

        public static FileSize operator -(FileSize first, FileSize second)
        {
            return new FileSize(first.DefaultValue - second.DefaultValue);
        }

        public bool Equals(FileSize other)
        {
            return _defValue == other._defValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != GetType()) return false;
            return Equals((FileSize)obj);
        }

        public override int GetHashCode()
        {
            return _defValue.GetHashCode();
        }
        #endregion == Equality ===================================================================================================================
    }
}
