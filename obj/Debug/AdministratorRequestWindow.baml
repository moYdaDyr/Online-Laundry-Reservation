   M S B A M L   `   `   `  ����E  AKursovaya2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Kursovaya2
Kursovaya2  5   	   +    %Kursovaya2.AdministratorRequestWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:Kursovaya2     5   	      m_admReqWindow  � Title#  �   5   	   $	��450��62   $	��800��6?   .��5	      �  � ColumnDefinitions 5
   
   �� 5      �� 5      �� 5       � RowDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��5* �6   �� 5      $��1* �6   .+�5   
   �� -   5        $'��!AdministratorRequestSelectorLabel��5      $��0��6=   $��0��6J   $��2��6Z     CommonLabelStyle#��[ 6n    ' #m_AdministratorRequestSelectorLabel#���  6�   �� 5   
   -   5        $#��AdministratorRequestTextLebel��5      $��2��69   $��0��6F   $��2��6V   #��[ 6j    # m_AdministratorRequestTextLebel#���  6�   �� 5   
   $��0��6   $��2��6    $��2��60   ��5      �� 5      -   5        $��AdministratorFiltersLabel��5      $��0��6=   $��2��6J     CommonTipStyle#��[ 6Z     m_AdministratorFiltersLabel#���  6�   .��5      �� .+�5       �� -   5        $!��AdministratorShowInProgress��5       $��1��6>   $��2��6K     CommonCheckBoxStyle#��[ 6[    ! m_AdministratorShowInProgress#���  6�   �� 5!      -   5        $��AdministratorShowDone��5!      $��2��68   $��2��6E   #��[ 6U     m_AdministratorShowDone#���  6�   5"      5$      �� 5&   
   -   5        $$��AdministratorQuitRequestButton��5&      $��4��67   $��0��6D    	 CommonButtonsStyle#��[	 6�    

 m_Quit#��� 
 6�   �� 5'   
   -   5        $$��AdministratorTakeRequestButton��5'      $��4��67   $��1��6D   #��[	 6�    $  m_AdministratorTakeRequestButton#���  6�   �� 5(   
   -   5        $(��"AdministratorCompleteRequestButton��5(      $��4��6;   $��2��6H   #��[	 6�    ( $m_AdministratorCompleteRequestButton#���  6�   �� 5*   
   -	   5        $#��AdministratorRequestsSelector��5*      $��1��68   $��0��6E   $��2��6U     CommonSelectorStyle#��[ 6�   �� 5,   
   $��Disabled=�6   $��Visible=�6@   $��3��6f   $��0��6s   $��3��6�   .��5-      �� -
   5        $��AdministratorRequestText��5-       �� 
IsReadOnly . 67   5.      5/      50      