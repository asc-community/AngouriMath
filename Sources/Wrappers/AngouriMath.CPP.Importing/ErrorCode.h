
struct ErrorCode
{
public:
    ~ErrorCode();
private:
    char* code;
    char* stackTrace;
};