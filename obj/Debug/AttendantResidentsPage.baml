   M S B A M L   `   `   `  ����E  AKursovaya2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Kursovaya2
Kursovaya2  5      '    !Kursovaya2.AttendantResidentsPage   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      *localclr-namespace:Kursovaya2     5        N� Title$  AttendantResidentsPage��5	       � 	Resources 5      ��%    5      .��5      �  � ColumnDefinitions 5   
   �� 5       � RowDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��6* �6   .+�5   
   �� -   5        $ ��AttendantResidentsTipLabel��5      $��0��66      m_AttendantResidentsTip#���   6C     CommonTipStyle#��[ 6t   �� 5   
   -   5        $"��AttendantResidentsTableLabel��5      $��1��68    " m_AttendantResidentsTableLabel#���  6E     CommonLabelStyle#��[ 6}   �� 5   
   $
��Auto=�6   $
��Auto=�6:   $��2��6_   .��5      &   System.Windows.Controls.DataGrid  -   5        $��AttendantResidentsTable��5         
IsReadOnly .    AutoGenerateColumns .     Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5         DataGridFilterHeaderStyle,  &System.Windows.Controls.DataGridColumn   HeaderStyle# [ 6K   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6)   �� 	 �� Path$	 Building�
   Header
 5!      �� 5"      -   5        $&�� AttendantResidentsBuildingHeader��5"   d    & "m_AttendantResidentsBuildingHeader#���  6(   6d   5$        5%      # [ 6J    6)   �� $	 Surname�
 5&      �� 5'       " m_AttendantResidentsNameHeader#���  6(   5)        5*      # [ 6G    6)   �� $
	 Name�
 5+      �� 5,       % !m_AttendantResidentsSurnameHeader#���  6(   5.        5/      # [ 6M    6)   �� $	 
Patronymic�
 50      �� 51       ( $m_AttendantResidentsPatronymicHeader#���  6(   53        54      # [ 6N    6)   �� $	 StudentCard�
 55      �� 56       )	 %m_AttendantResidentsStudentCardHeader#��� 	 6(   58        59      # [ 6G    6)   �� $
	 Room�
 5:      �� 5;       "
 m_AttendantResidentsRoomHeader#��� 
 6(   5=        5>      # [ 6M    6)   �� $	 
StatusText�
 5?      �� 5@       $  m_AttendantResidentsStatusHeader#���  6(   5B      4  .System.Windows.Controls.DataGridTemplateColumn  5D         CellTemplate 5E      �� 5F      .R�5G   "   �� $(��"AttendantResidentsTableBlockButton��6)   -   5         ( $m_attendantResidentsTableBlockButton#���  5G   �   ��5H   )   �� *ID+5I      5K        5M       5N      �� 5O      .R�5P   "   �� $*��$AttendantResidentsTableUnblockButton��6)   -   5         * &m_attendantResidentsTableUnblockButton#���  5P   �   ��5Q   )   �� *ID+5R      5T        5V       5W      �� 5X      .R�5Y   "   �� $*��$AttendantResidentsTableReportkButton��6)   -   5         ) %m_attendantResidentsTableReportButton#���  5Y   �   ��5Z      �� *ID+5[      5]      5_      5`      5a      5c      