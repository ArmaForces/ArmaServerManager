using System;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content.Exceptions
{
    /// <summary>
    /// Exception thrown when item with given ID
    /// </summary>
    public class WorkshopItemNotExistsException : Exception
    {
        private const string DefaultMessageTemplate = "Item with {0} id could not be found in Workshop.";

        public uint ItemId { get; }

        public WorkshopItemNotExistsException(uint itemId) : this(itemId, GetDefaultMessage(itemId))
        {
        }

        public WorkshopItemNotExistsException(uint itemId, Exception innerException) : this(
            itemId,
            GetDefaultMessage(itemId),
            innerException)
        {
        }

        public WorkshopItemNotExistsException(uint itemId, string message) : base(message)
        {
            ItemId = itemId;
        }

        public WorkshopItemNotExistsException(uint itemId, string message, Exception innerException) : base(message, innerException)
        {
            ItemId = itemId;
        }

        private static string GetDefaultMessage(uint itemId) => string.Format(DefaultMessageTemplate, itemId);
    }
}
