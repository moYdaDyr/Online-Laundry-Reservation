   M S B A M L   `   `   `  ����E  AKursovaya2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Kursovaya2
Kursovaya2  5      &     Kursovaya2.StudentRedactBookPage   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      *localclr-namespace:Kursovaya2     5        N� Title$  StudentRedactBookPage��5	      .��5      �  � RowDefinitions 5   
   �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5       � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��2* �6   .+�5   
   �� -   5        $��ChooseYourBookLabel��5      $��0��60   $��0��6@   $��2��6M      m_selectBook#���   6a     CommonLabelStyle#��[ 6�   �� 5   
   -   5        $��StudentBookSelector��5      $��0��6.   $��1��6>   $��2��6K     CommonSelectorStyle#��[ 6�   5       �� 5"   
   -   5        $��StudentRedactLaundryLabel��5"      $��0��66   $��2��6F     	m_laundry#���  6S   #��[ 6v   �� 5#   
   -   5        $��StudentRedactLaundryBox��5#      $��1��62   $��2��6B    �� 
IsReadOnly .   InfoTextBoxStyle#��[ 6a   �� 5$   
   -   5        $��StudentRedactDateLabel��5$      $��0��63   $��3��6C    
 m_date#���  6P   #��[ 6p   �� 5%   
   -   5        $��StudentRedactDateBox��5%      $��1��6/   $��3��6?    . #��[ 6^   �� 5'   
   -   5        $��StudentRedactTimeLabel��5'      $��0��63   $��4��6C   $��2��6P     m_studentBookTime#���  6d   #��[ 6�   �� 5(   
   -   5        $��StudentTimeSelector��5(      $��0��6.   $��5��6>   $��2��6K   #��[ 6�   5*      �� 5,   
   -	   5        $ ��StudentRedactDurationLabel��5,      $��0��67   $��6��6G   $��2��6T     m_studentBookCont#���  6h   #��[ 6�   �� 5-   
   -
   5        $��StudentDurationSelector��5-      $��0��62   $��7��6B   $��2��6O   #��[ 6�   5/      �� 51   
   -   5        $��StudentRedactUpdateButton��51      $��0��6Z   $��8��6j     m_redactBooking#���  6w    	 CommonButtonsStyle#��[	 6�   �� 52   
   -   5        $��StudentReductDeleteButton��52      $��1��6Z   $��8��6j    
 m_deleteBooking#��� 
 6w   #��[	 6�   �� 54   
   -   5        $��FreeTimeHeaderLable��54      $��0��60   $��2��6=     m_studentBookTableHeader#���  6M     CommonHeaderStyle#��[ 6   �� 56   
   -   5        $��FreeTimeTableBox��56      $��1��61   $��2��6>   $��6��6N   $
��Auto=�6_   $
��Auto=�6�   .��57      � -   5        $��FreeTimeTable��57      58      59      �� 5<   
   -   5        $��FreeTimeTipLable��5<      $��7��6-   $��2��6:   $��2��6J     m_studentBookTableTip#���  6[     CommonTipStyle#��[ 6�   5>      5?      