import re
import string
import transliterate

punctuation_to_space = string.punctuation.replace("'", "")
space_translation = str.maketrans(dict.fromkeys(punctuation_to_space, " "))
delete_translation = str.maketrans(dict.fromkeys("’«»", ""))

def normalize_ukrainian_text(text: str) -> str:
    """Processes ukrainian text string into a text string that is ready to be used inside a prediction model."""
    text = text.translate(space_translation).translate(delete_translation)
    
    res = transliterate.translit(text, language_code='uk', reversed=True)
    
    # Add spaces before capital letters (for camelCase words)
    res = re.sub(r'(?<=[a-z])(?=[A-Z])', ' ', res).lower()

    words = res.split()
    processed_words = []
    
    for word in words:
        if len(word) < 2:
            continue
            
        if any(char.isdigit() for char in word):
            continue
            
        if not re.fullmatch(r"[a-zA-Z'-]{3,40}", word):
            continue
            
        processed_words.append(word)
    
    return ' '.join(processed_words)