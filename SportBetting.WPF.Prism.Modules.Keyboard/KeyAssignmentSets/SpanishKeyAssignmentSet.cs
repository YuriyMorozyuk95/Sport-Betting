namespace SportBetting.WPF.Prism.Modules.Keyboard.KeyAssignmentSets
{
    /// <summary>
    /// The SpanishKeyAssignments subclass of KeyAssignmentSet overrides those KeyAssignments that need to differ
    /// from those of the English keyboard.
    /// </summary>
    public class SpanishKeyAssignmentSet : KeyAssignmentSet
    {
        public SpanishKeyAssignmentSet()
            : base()
        {
        }

        #region KeyboardLayout
        /// <summary>
        /// Get the WhichKeyboardLayout this KeyAssignmentSet is used for.
        /// </summary>
        public override WhichKeyboardLayout KeyboardLayout
        {
            get { return WhichKeyboardLayout.Spanish; }
        }
        #endregion

        #region The individual key assignments

        #region 11: VK_OEM_MINUS
        /// <summary>
        /// 11  Apostrophe/Question mark
        /// </summary>
        public override KeyAssignment VK_OEM_MINUS
        {
            get
            {
                if (_VK_OEM_MINUS == null)
                {
                    _VK_OEM_MINUS = new KeyAssignment(0x0027, 0x003F, "Apostrophe", "Question mark", false);
                }
                return _VK_OEM_MINUS;
            }
        }
        #endregion

        #region 12: VK_OEM_PLUS
        /// <summary>
        /// 12
        /// </summary>
        public override KeyAssignment VK_OEM_PLUS
        {
            get
            {
                if (_VK_OEM_PLUS == null)
                {
                    _VK_OEM_PLUS = new KeyAssignment(0x00A1, 0x00BF, "A1 InvertedExclamation mark", "BF Inverted Question mark", false, true);
                }
                return _VK_OEM_PLUS;
            }
        }
        #endregion

        #region 35: VK_OEM_1
        /// <summary>
        /// 35  N Tilde
        /// </summary>
        public override KeyAssignment VK_OEM_1
        {
            get
            {
                if (_VK_OEM_1 == null)
                {
                    _VK_OEM_1 = new KeyAssignment(0x00F1, 0x00D1, "n tilde", "N Tilde", true, false);
                }
                return _VK_OEM_1;
            }
        }
        #endregion

        #region 36: VK_OEM_7
        /// <summary>
        /// 36  c-cedilla, used in the Albanian, Azerbaijani, Kurdish (Kurmanji dialect), Ligurian, Tatar, Turkish, Turkmen, and Zazaki alphabets.
        /// This letter also appears in Catalan, French, Friulian, Occitan, and Portuguese as a variant of the letter c.
        /// In the IPA, /(c-cedilla)/ represents the voiceless palatal fricative.
        /// </summary>
        public override KeyAssignment VK_OEM_7
        {
            get
            {
                if (_VK_OEM_7 == null)
                {
                    _VK_OEM_7 = new KeyAssignment(0x00E7, 0x00C7, "minuscule c-cedilla", "Majuscule c-cedilla", true, false);
                }
                return _VK_OEM_7;
            }
        }
        #endregion

        #region 44: VK_OEM_COMMA
        /// <summary>
        /// 44  Commas/Semicolon
        /// </summary>
        public override KeyAssignment VK_OEM_COMMA
        {
            get
            {
                if (_VK_OEM_COMMA == null)
                {
                    _VK_OEM_COMMA = new KeyAssignment(0x002C, 0x003B, "Comma", "Semicolon", false);
                }
                return _VK_OEM_COMMA;
            }
        }
        #endregion

        #region 45: VK_OEM_PERIOD
        /// <summary>
        /// 45  Period/Semicolon
        /// </summary>
        public override KeyAssignment VK_OEM_PERIOD
        {
            get
            {
                if (_VK_OEM_PERIOD == null)
                {
                    _VK_OEM_PERIOD = new KeyAssignment(0x002E, 0x003B, "Period", "Semicolon", false, true);
                }
                return _VK_OEM_PERIOD;
            }
        }
        #endregion

        #region 46: VK_OEM_2
        /// <summary>
        /// 46  Hyphen/Underline
        /// </summary>
        public override KeyAssignment VK_OEM_2
        {
            get
            {
                if (_VK_OEM_2 == null)
                {
                    _VK_OEM_2 = new KeyAssignment(0x002D, 0x005F, "Hyphen", "Underline", false, true);
                }
                return _VK_OEM_2;
            }
        }
        #endregion

        #endregion The individual key assignments
    }
}
