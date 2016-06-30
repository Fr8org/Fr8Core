def enum(**enums):
    return type('Enum', (object,), enums)
