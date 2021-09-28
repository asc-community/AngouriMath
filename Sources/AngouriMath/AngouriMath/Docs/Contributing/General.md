## General information for contributors

AM's class structure is a hierarchy with `Entity` as the ultimate class. All class-nodes are defined
inside of `Entity`. Those that can be simplifed to a number are inherited from `NumericNode`, but are not
located in this class. Those that can be simplified to a boolean are inherited from `BooleanNode`, but are not
located in this class.