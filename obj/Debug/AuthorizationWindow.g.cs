﻿#pragma checksum "..\..\AuthorizationWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "973D1468BC1AABD07DBA667FB5B327D0D32D6541CFE652B4FAB5E3F684FDCB55"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using Kursovaya2;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Kursovaya2 {
    
    
    /// <summary>
    /// AuthorizationWindow
    /// </summary>
    public partial class AuthorizationWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\AuthorizationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock AuthLoginLabel;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\AuthorizationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox AuthLoginTextBox;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\AuthorizationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock AuthCorpusLabel;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\AuthorizationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox AuthCorpusSelector;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\AuthorizationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock AuthPasswordLabel;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\AuthorizationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox AuthPasswordTextBox;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\AuthorizationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AuthCancelButtom;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\AuthorizationWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AuthAuthorButtom;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Kursovaya2;component/authorizationwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\AuthorizationWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.AuthLoginLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.AuthLoginTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 23 "..\..\AuthorizationWindow.xaml"
            this.AuthLoginTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.AuthLoginTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.AuthCorpusLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.AuthCorpusSelector = ((System.Windows.Controls.ComboBox)(target));
            
            #line 26 "..\..\AuthorizationWindow.xaml"
            this.AuthCorpusSelector.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.AuthCorpusSelector_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.AuthPasswordLabel = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.AuthPasswordTextBox = ((System.Windows.Controls.PasswordBox)(target));
            
            #line 31 "..\..\AuthorizationWindow.xaml"
            this.AuthPasswordTextBox.PasswordChanged += new System.Windows.RoutedEventHandler(this.AuthPasswordTextBox_PasswordChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.AuthCancelButtom = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\AuthorizationWindow.xaml"
            this.AuthCancelButtom.Click += new System.Windows.RoutedEventHandler(this.AuthCancel_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.AuthAuthorButtom = ((System.Windows.Controls.Button)(target));
            
            #line 35 "..\..\AuthorizationWindow.xaml"
            this.AuthAuthorButtom.Click += new System.Windows.RoutedEventHandler(this.AuthAuthor_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
