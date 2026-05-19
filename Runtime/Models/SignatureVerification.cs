using System;
using UnityEngine;

namespace CrashGuard
{
    /// <summary>
    /// Result of signature verification
    /// </summary>
    [Serializable]
    public class SignatureVerification
    {
        /// <summary>
        /// The verified signature
        /// </summary>
        public string signature;

        /// <summary>
        /// Whether the signature is flagged as problematic (known moderator)
        /// </summary>
        public bool isFlagged;

        /// <summary>
        /// Error message if verification failed
        /// </summary>
        public string error;

        /// <summary>
        /// Whether the verification was successful
        /// </summary>
        public bool IsSuccess => string.IsNullOrEmpty(error);

        /// <summary>
        /// Create SignatureVerification from JSON string
        /// </summary>
        public static SignatureVerification FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new SignatureVerification { error = "Empty response" };
            }

            try
            {
                return JsonUtility.FromJson<SignatureVerification>(json);
            }
            catch (Exception e)
            {
                return new SignatureVerification { error = e.Message };
            }
        }

        public override string ToString()
        {
            return $"SignatureVerification(signature={signature}, isFlagged={isFlagged})";
        }
    }
}
