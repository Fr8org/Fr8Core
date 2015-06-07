Notes on the DDay Library

Put things here that are no obvious but might be useful to those who follow.





Parsing DateTimes
The docs claim that #Parse is supported on ICalDateTime, but it's not implemented. However, if you pass an ICS calendar date string into the constructor, it will carry out the parse:
   iCalDateTime DTStart = new iCalDateTime("20040117")'