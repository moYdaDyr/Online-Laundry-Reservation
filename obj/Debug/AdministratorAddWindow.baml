   M S B A M L   `   `   `  ����E  AKursovaya2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Kursovaya2
Kursovaya2  5   	   '    !Kursovaya2.AdministratorAddWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:Kursovaya2     5   	     � Title$  AdministratorAddWindow��5   	   $
��Auto��6(   $
��Auto��66    � SizeToContent$ WidthAndHeight=�6C   .��5	      �  � ColumnDefinitions 5
   
   �� 5      �� 5       � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� -   5        $!��AdministratorAddWindowLabel��5         CommonLabelStyle#��[  67   $��0��6a   $��0��6q   $��2��6~    ! m_AdministratorAddWindowLabel#���  6�   �� 5   
   -   5        $"��AdministratorAddCancelButton��5        CommonButtonsStyle#��[ 65   $��0��6a   $��2��6q     m_Cancel#���  6~   6�   �� 5   
   -   5        $��AdministratorAddlButton��5      #��[ 60   $��1��6\   $��2��6l     
m_SaveData#���  6y   6�   &   System.Windows.Controls.DataGrid  5   
   -   5        $��AdministratorTable��5      $��0��6-   $��1��6=   $��2��6J      AutoGenerateColumns .  5      5      5      