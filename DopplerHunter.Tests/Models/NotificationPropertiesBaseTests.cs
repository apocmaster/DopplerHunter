using DopplerHunter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Tests.Models
{
    public class TestNotificationClass: NotificationPropertiesBase
    {
        private string _name = string.Empty;

        public string Name
        {
            get { return _name; }
            set 
            { 
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public class NotificationPropertiesBaseTests
    {
        [Fact]
        public void SetProperty_RaisesPropertyChangedEvent() 
        { 
            // Arrange
            var testObj = new TestNotificationClass();
            string? propertyChangedName = null;

            testObj.PropertyChanged += (sender, args) =>
            {
                propertyChangedName = args.PropertyName;
            };

            // Act
            testObj.Name = "Test Value";

            // Assert
            Assert.Equal("Name", propertyChangedName);

        }
    }



}
