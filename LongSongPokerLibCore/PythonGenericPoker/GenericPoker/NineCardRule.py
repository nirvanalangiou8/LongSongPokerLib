from GenericPoker.BaseCardRule import BaseCardRule


class NineCardRule(BaseCardRule):
    _default_instance = None

    def __init__(self):
        super().__init__()
        self.min_straight_count = 5
        self.min_flush_count = 5
        self.min_flush_straight_count = 3
        self.min_kind_count = 2
        self.card_count = 9

    @classmethod
    def default(cls) -> 'NineCardRule':
        if cls._default_instance is None:
            cls._default_instance = NineCardRule()
        return cls._default_instance
