import re

def resolve_users_controller():
    path = "ChatApp.WebApi/Controllers/UsersController.cs"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()
    
    # Block 1: Using statements
    content = re.sub(r'<<<<<<< HEAD\n(.*?)\n=======\n(.*?)\n>>>>>>> origin/main', 
                     lambda m: m.group(1) + "\n" + m.group(2) if "using System.Linq;" in m.group(2) else m.group(0), 
                     content, flags=re.DOTALL)
    
    # Block 2: Class body
    def resolve_class_body(m):
        head = m.group(1)
        origin = m.group(2)
        
        # Extract the GetDemoUsers method from origin
        match = re.search(r'(/// <summary>Demo accounts.*?)', origin, flags=re.DOTALL)
        demo_method = match.group(1) if match else ""
        
        # We keep HEAD's entire body, and append demo_method
        return head + "\n\n    " + demo_method.strip() + "\n"

    content = re.sub(r'<<<<<<< HEAD\n(\[Authorize\].*?)\n=======\n(.*?)\n>>>>>>> origin/main', resolve_class_body, content, flags=re.DOTALL)
    
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)


def resolve_database_seeder():
    path = "ChatApp.WebApi/Extensions/DatabaseSeeder.cs"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()
    
    def resolve_seeder(m):
        head = m.group(1)
        return head
        
    content = re.sub(r'<<<<<<< HEAD\n(.*?)\n=======\n.*?\n>>>>>>> origin/main', resolve_seeder, content, flags=re.DOTALL)
    
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)


def resolve_chathub():
    path = "ChatApp.WebApi/Hubs/ChatHub.cs"
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()
    
    # Block 1: Using statements
    content = re.sub(r'<<<<<<< HEAD\n(.*?)\n=======\n(.*?)\n>>>>>>> origin/main', 
                     lambda m: m.group(1) + "\n" + m.group(2), 
                     content, count=1, flags=re.DOTALL)
                     
    # Block 2: HEAD has methods, origin is empty
    content = re.sub(r'<<<<<<< HEAD\n(.*?)\n=======\n>>>>>>> origin/main', 
                     lambda m: m.group(1), 
                     content, flags=re.DOTALL)
                     
    # Block 3: HEAD has WebRTC, origin has Message commands
    content = re.sub(r'<<<<<<< HEAD\n(.*?)\n=======\n(.*?)\n>>>>>>> origin/main', 
                     lambda m: m.group(2) + "\n\n" + m.group(1), 
                     content, flags=re.DOTALL)

    with open(path, "w", encoding="utf-8") as f:
        f.write(content)

resolve_users_controller()
resolve_database_seeder()
resolve_chathub()
