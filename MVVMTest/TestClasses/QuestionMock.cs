﻿using System;
using SharedInterfaces;

namespace MVVMTest
{
    public class QuestionMock : IQuestionWindowService
    {
        public void ShowMessage(string text, string yesButtonText, string noButtonText, EventHandler yesClick, EventHandler noClick,
                                bool IsVisibleNoButton = true, int yesButtonTimer = 0, bool clearCashToTransfer = false, bool warning = false)
        {
            yesClick(null, null);
        }

        public void ShowMessage(string text)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}